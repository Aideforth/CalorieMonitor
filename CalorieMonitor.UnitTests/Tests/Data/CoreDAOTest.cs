using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Data.Implementations;
using CalorieMonitor.UnitTests.Mocks.Data;
using CalorieMonitor.UnitTests.Mocks.External;
using CalorieMonitor.UnitTests.Wrappers;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CalorieMonitor.UnitTests.Tests.Data
{
    public class CoreDAOTest
    {
        readonly CoreDAOWrapper<Entity> coreDAO;
        readonly MockDbConnection mockDbConnection;
        readonly MockDbConnectionProvider mockDbConnectionprovider;
        readonly CRUDQueryObject queryObject;
        readonly DateTime currentTime = DateTime.UtcNow;


        public CoreDAOTest()
        {
            mockDbConnection = new MockDbConnection();
            queryObject = new CRUDQueryObject
            {
                DeleteQuery = "DeleteQuery @Id",
                InsertQuery = "InsertQuery @DateCreated",
                SelectQuery = "SelectQuery @Id",
                UpdateQuery = "UpdateQuery @Id @DateCreated @DateUpdated"
            };
            mockDbConnectionprovider = new MockDbConnectionProvider();
            mockDbConnectionprovider.MockGetDbConnectionAsync(mockDbConnection);

            coreDAO = new CoreDAOWrapper<Entity>(mockDbConnectionprovider.Object, queryObject);
        }

        [Fact]
        public void Constructor_NullConnectionProviderArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() =>
                                    new CoreDAOWrapper<Entity>(null, queryObject));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("connectionProvider", (exception as ArgumentNullException).ParamName);

        }

        [Fact]
        public void Constructor_NullCRUDQueryObjectArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() =>
                                    new CoreDAOWrapper<Entity>(mockDbConnectionprovider.Object, null));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("queryObject", (exception as ArgumentNullException).ParamName);

        }

        [Fact]
        public async Task DeleteAsync_Valid_ReturnsTrue()
        {
            //arrange
            long id = 2;
            mockDbConnection.MockExecuteAsync(queryObject.DeleteQuery);

            //Act
            bool response = await coreDAO.DeleteAsync(id);

            //Assert
            Assert.True(response);
            RunValidationsWhenIdIsUsedInQueryConditions(id);
        }

        [Fact]
        public async Task DeleteAsync_Error_ThrowsException()
        {
            //arrange
            long id = 2;
            mockDbConnection.MockExecuteAsyncWithException(queryObject.DeleteQuery);

            //Act
            Exception exception = await Record.ExceptionAsync(() => coreDAO.DeleteAsync(id));

            //Assert
            RunValidationsAfterException(exception);
        }

        [Fact]
        public async Task GetAsync_Valid_ReturnsTrue()
        {
            //arrange
            long id = 2;
            Entity returnValue = new Entity { Id = id, DateCreated = currentTime, DateUpdated = currentTime };
            mockDbConnection.MockQueryFirstOrDefaultAsync(queryObject.SelectQuery, returnValue);

            //Act
            Entity response = await coreDAO.GetAsync(id);

            //Assert
            RunValidationsWhenEntityIsReturned(response, id);
        }

        [Fact]
        public async Task GetAsync_Error_ThrowsException()
        {
            //arrange
            long id = 2;
            mockDbConnection.MockQueryFirstOrDefaultAsyncWithException(queryObject.SelectQuery);

            //Act
            Exception exception = await Record.ExceptionAsync(() => coreDAO.GetAsync(id));

            //Assert
            RunValidationsAfterException(exception);
        }
        [Fact]
        public async Task UpdateAsync_Valid_ReturnsTrue()
        {
            //arrange
            long id = 2;
            Entity updateValue = new Entity { Id = id, DateCreated = currentTime, DateUpdated = currentTime };
            mockDbConnection.MockExecuteAsync(queryObject.UpdateQuery);

            //Act
            Entity response = await coreDAO.UpdateAsync(updateValue);

            //Assert
            Assert.Same(updateValue, response);
            Assert.Equal(1, mockDbConnection.Parameters.Count(c => c.ParameterName == "DateCreated" && (DateTime)c.Value == currentTime));
            Assert.Equal(1, mockDbConnection.Parameters.Count(c => c.ParameterName == "DateUpdated" && (DateTime)c.Value == currentTime));
            RunValidationsWhenEntityIsReturned(response, id);
        }

        [Fact]
        public async Task UpdateAsync_Error_ThrowsException()
        {
            //arrange
            Entity updateValue = new Entity { Id = 2, DateCreated = DateTime.Now };
            mockDbConnection.MockExecuteAsyncWithException(queryObject.UpdateQuery);

            //Act
            Exception exception = await Record.ExceptionAsync(() => coreDAO.UpdateAsync(updateValue));

            //Assert
            RunValidationsAfterException(exception);
        }

        [Fact]
        public async Task InsertAsync_Valid_ReturnsTrue()
        {
            //arrange
            long id = 2;
            Entity insertValue = new Entity { DateCreated = currentTime };
            mockDbConnection.MockExecuteScalarAsync(queryObject.InsertQuery, id);

            //Act
            Entity response = await coreDAO.InsertAsync(insertValue);

            //Assert
            Assert.Same(insertValue, response);
            Assert.NotNull(response);
            Assert.Equal(id, response.Id);
            Assert.Equal(currentTime, response.DateCreated);
            RunVerfications();
        }

        [Fact]
        public async Task InsertAsync_Error_ThrowsException()
        {
            //arrange
            Entity insertValue = new Entity { DateCreated = DateTime.Now };
            mockDbConnection.MockExecuteScalarAsyncWithException(queryObject.InsertQuery);

            //Act
            Exception exception = await Record.ExceptionAsync(() => coreDAO.InsertAsync(insertValue));

            //Assert
            RunValidationsAfterException(exception);
        }

        [Fact]
        public void GenerateParameters_Valid_ReturnParameters()
        {
            //Arrange
            List<QueryParam> queryParams = GenerateQueryParamsForTest();

            //Act
            DynamicParameters parameters = coreDAO.GenerateParameters(queryParams);

            //Assert
            ValidateGenerateParametersTest(parameters);
        }

        [Fact]
        public void GenerateSearchParameters_Valid_ReturnParameters()
        {
            //arrange
            List<QueryParam> queryParams = GenerateQueryParamsForTest();
            int startIndex = 0;
            int limit = 10;

            //Act
            DynamicParameters parameters = coreDAO.GenerateSearchParameters(queryParams, startIndex, limit);

            //Assert
            ValidateGenerateParametersTest(parameters);
            Assert.Equal(startIndex, parameters.Get<int>("Start"));
            Assert.Equal(limit, parameters.Get<int>("Limit"));
        }

        private static void ValidateGenerateParametersTest(DynamicParameters parameters)
        {
            Assert.Equal("Named Well", parameters.Get<string>("Name"));
            Assert.Equal(10, parameters.Get<int>("Age"));
        }

        private static List<QueryParam> GenerateQueryParamsForTest()
        {
            return new List<QueryParam>
            {
                new QueryParam
                {
                    Name = "Name",
                    DbType = DbType.String,
                    Value = "Named Well"
                },
                new QueryParam
                {
                    Name = "Age",
                    DbType = DbType.Int32,
                    Value = 10
                }
            };
        }

        private void RunValidationsWhenEntityIsReturned(Entity entity, long id)
        {
            Assert.NotNull(entity);
            Assert.Equal(id, entity.Id);
            Assert.Equal(currentTime, entity.DateCreated);
            Assert.Equal(currentTime, entity.DateUpdated);
            RunValidationsWhenIdIsUsedInQueryConditions(id);
        }
        private void RunValidationsWhenIdIsUsedInQueryConditions(long id)
        {
            Assert.Equal(1, mockDbConnection.Parameters.Count(c => c.ParameterName == "Id" && (long)c.Value == id));
            RunVerfications();
        }

        private void RunValidationsAfterException(Exception exception)
        {
            Assert.IsType<SqlException>(exception);
            RunVerfications();
        }

        private void RunVerfications()
        {
            mockDbConnection.RunVerification();
            mockDbConnectionprovider.RunVerification();
        }
    }
}
