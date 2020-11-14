using CalorieMonitor.Core.Entities;
using CalorieMonitor.UnitTests.Utilities;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CalorieMonitor.UnitTests.Mocks.External
{
    public class MockDbConnection : Mock<IDbConnection>
    {
        public List<DbParameter> Parameters;
        public DbParameterCollection parameterList;
        private Mock<DbCommand> commandMock;
        private bool hasParametersSet;
        protected List<Action> verifications;

        public MockDbConnection()
        {
            Parameters = new List<DbParameter>();
            verifications = new List<Action>();
        }
        public void MockExecuteAsync(string sql)
        {
            Mock<DbCommand> mockCommand = SetupCommand(sql);
            mockCommand.Setup(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            verifications.Add(() =>
                mockCommand.Verify(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Once())
                );
        }
        public void MockExecuteAsyncWithException(string sql)
        {
            Mock<DbCommand> mockCommand = SetupCommand(sql);
            mockCommand.Setup(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Throws(SqlExceptionUtil.Get());
            verifications.Add(() =>
                mockCommand.Verify(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Once())
                );
        }

        public void MockQueryFirstOrDefaultAsync(string sql, Entity item)
        {
            MockQueryAsync<Entity>(sql, item == null ? new List<Entity>() : new List<Entity> { item });
        }

        public void MockQueryFirstOrDefaultAsyncWithException(string sql)
        {
            MockQueryAsyncWithException(sql);
        }

        public void MockQueryAsync<T>(string sql, List<T> items, bool hasParameters = true)
        {
            var properties = typeof(T).GetProperties();

            Mock<DbDataReader> dataReader = new Mock<DbDataReader>();
            var columnNames = GetColumnNamesAndTypes(properties);
            dataReader.Setup(v => v.GetName(It.IsAny<int>())).Returns<int>(num => columnNames[num].Key);
            dataReader.Setup(v => v.GetFieldType(It.IsAny<int>())).Returns<int>(num => columnNames[num].Value);

            dataReader.SetupGet(v => v.FieldCount).Returns(() => columnNames.Count);
            SetUpColumnGetMethods(dataReader, properties, items);

            IProtectedMock<DbCommand> mockCommand = SetupCommand(sql, hasParameters).Protected();
            mockCommand.Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                           .ReturnsAsync(() => dataReader.Object);

            verifications.Add(() =>
                mockCommand.Verify("ExecuteDbDataReaderAsync", Times.Once(), ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                );
        }

        private List<KeyValuePair<string, Type>> GetColumnNamesAndTypes(PropertyInfo[] properties, List<KeyValuePair<string, Type>> columnNames = null)
        {
            List<Action> getPropertyEntityNameActions = new List<Action>();
            if (columnNames == null) columnNames = new List<KeyValuePair<string, Type>>();
            //exclude lists
            properties = properties.Where(v => !v.PropertyType.IsGenericType || v.PropertyType.GetGenericTypeDefinition() != typeof(List<>)).ToArray();

            foreach (var property in properties)
            {
                if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(string) || property.PropertyType.IsEnum)
                {
                    columnNames.Add(new KeyValuePair<string, Type>(property.Name, property.PropertyType));
                }
                else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    Type propertyType = property.PropertyType.GetGenericArguments()[0];
                    columnNames.Add(new KeyValuePair<string, Type>(property.Name, propertyType));
                }
                else
                {
                    getPropertyEntityNameActions.Add(() => GetColumnNamesAndTypes(property.PropertyType.GetProperties(), columnNames));
                }
            }
            getPropertyEntityNameActions.ForEach(act => act());
            return columnNames;
        }
        private int SetUpColumnGetMethods<T>(Mock<DbDataReader> dataReader, PropertyInfo[] properties, List<T> items, int currentCount = 0)
        {
            List<Func<int, int>> getPropertyNamesForClassPropertyActions = new List<Func<int, int>>();
            if (currentCount == 0)
            {
                SetUpMockDataReaderReadCalls(dataReader, items);
            }
            //exclude lists
            properties = properties.Where(v => !v.PropertyType.IsGenericType || v.PropertyType.GetGenericTypeDefinition() != typeof(List<>)).ToArray();

            foreach (var property in properties)
            {

                if (property.PropertyType.IsPrimitive ||
                    property.PropertyType == typeof(DateTime) ||
                    property.PropertyType == typeof(string) ||
                    property.PropertyType.IsEnum ||
                    (property.PropertyType.IsGenericType 
                    && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    currentCount = SetUpMockReturnValuesForDataReader(dataReader, items, currentCount, property);
                }
                else
                {
                    Func<int, int> getPropertyNamesForClassPropertyAction = SetUpGetPropertyNamesForClassPropertyAction(dataReader, items, property);
                    getPropertyNamesForClassPropertyActions.Add(getPropertyNamesForClassPropertyAction);
                }
            }
            foreach (var action in getPropertyNamesForClassPropertyActions)
            {
                currentCount = action(currentCount);
            }

            return currentCount;
        }

        private static int SetUpMockReturnValuesForDataReader<T>(Mock<DbDataReader> dataReader, List<T> items, int currentCount, PropertyInfo property)
        {
            var columnDef = dataReader.As<IDataReader>().SetupSequence(b => b[currentCount]);
            items.ForEach(v =>
            {
                if (v != null)
                {
                    var value = property.GetMethod.Invoke(v, null);
                    if (value != null)
                    {
                        if (property.PropertyType.IsEnum)
                        {
                            columnDef.Returns((int)value);
                        }
                        else
                        {
                            columnDef.Returns(value);
                        }
                        return;
                    }
                }
                columnDef.Returns(DBNull.Value);
            });
            currentCount++;
            return currentCount;
        }

        private Func<int, int> SetUpGetPropertyNamesForClassPropertyAction<T>(Mock<DbDataReader> dataReader,
            List<T> items,
            PropertyInfo property)
        {
            var newItems = items.Select(v =>
            {
                return property.GetMethod.Invoke(v, null);
            }).ToList();

            return (count) =>
            {
                return SetUpColumnGetMethods(dataReader,
                    property.PropertyType.GetProperties(),
                    newItems,
                    count);
            };
        }

        private static void SetUpMockDataReaderReadCalls<T>(Mock<DbDataReader> dataReader, List<T> items)
        {
            var columnDef = dataReader.As<IDataReader>().SetupSequence(b => b.Read());
            var columnDef2 = dataReader.SetupSequence(b => b.ReadAsync(It.IsAny<CancellationToken>()));
            items.ForEach(v =>
            {
                columnDef.Returns(() => true);
                columnDef2.ReturnsAsync(() => true);
            });
            columnDef.Returns(false);
            columnDef2.ReturnsAsync(false);
        }

        public void MockQueryAsyncWithException(string sql, bool hasParameters = true)
        {
            IProtectedMock<DbCommand> mockCommand = SetupCommand(sql, hasParameters).Protected();
            mockCommand.Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                          .Throws(SqlExceptionUtil.Get());

            verifications.Add(() =>
                mockCommand.Verify("ExecuteDbDataReaderAsync", Times.Once(), ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                );
        }

        public void MockExecuteScalarAsync<T>(string sql, T item, bool hasParameters = true)
        {
            Mock<DbCommand> mockCommand = SetupCommand(sql, hasParameters);
            mockCommand.Setup(x => x.ExecuteScalarAsync(It.IsAny<CancellationToken>())).ReturnsAsync(item);

            verifications.Add(() =>
                mockCommand.Verify(x => x.ExecuteScalarAsync(It.IsAny<CancellationToken>()), Times.Once())
                );
        }

        public void MockExecuteScalarAsyncWithException(string sql)
        {
            Mock<DbCommand> mockCommand = SetupCommand(sql);
            mockCommand.Setup(x => x.ExecuteScalarAsync(It.IsAny<CancellationToken>())).Throws(SqlExceptionUtil.Get());

            verifications.Add(() =>
                mockCommand.Verify(x => x.ExecuteScalarAsync(It.IsAny<CancellationToken>()), Times.Once())
                );
        }

        private Mock<DbCommand> SetupCommand(string sql, bool hasParameters = true)
        {
            if (commandMock == null)
            {
                commandMock = new Mock<DbCommand>();
                Setup(m => m.CreateCommand()).Returns(() =>
                commandMock.Object);
                SetupGet(c => c.State).Returns(ConnectionState.Open);

                commandMock.SetupSet(v => v.CommandText = sql).Verifiable();
            }
            if (hasParameters && !hasParametersSet)
            {
                commandMock.Protected()
                           .SetupGet<DbParameterCollection>("DbParameterCollection")
                           .Returns(() => new Mock<DbParameterCollection>().Object);

                commandMock.Protected()
                           .Setup<DbParameter>("CreateDbParameter")
                           .Returns(() =>
                           {
                               var parameter = new Mock<DbParameter>().SetupAllProperties().Object;
                               Parameters.Add(parameter);
                               return parameter;
                           });
                hasParametersSet = true;
            }
            return commandMock;
        }
        public void RunVerification()
        {
            VerifyAll();
            verifications.ForEach(v => v());
            commandMock.VerifyAll();
        }
    }
}
