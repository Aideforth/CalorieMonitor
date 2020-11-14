using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Enums;
using System;
using System.Collections.Generic;
using Xunit;

namespace CalorieMonitor.UnitTests.Utilities
{
    public class UserUtil
    {
        public static Action<User>[] GenerateVerifications(List<User> records)
        {
            int count = records.Count;
            Action<User>[] verifications = new Action<User>[count];
            for (int i = 1; i <= count; i++)
            {
                var record = records[i - 1];
                verifications[i - 1] = item =>
                {
                    Assert.Equal(record.Id, item.Id);
                    Assert.Equal(record.DateCreated, item.DateCreated);
                    Assert.Equal(record.FirstName, item.FirstName);
                    Assert.Equal(record.LastName, item.LastName);
                    Assert.Equal(record.Password, item.Password);
                    Assert.Equal(record.Role, item.Role);
                    Assert.Equal(record.UserName, item.UserName);
                    Assert.Equal(record.EmailAddress, item.EmailAddress);
                };
            }

            return verifications;
        }

        public static List<User> GenerateRecords(DateTime currentTime, int count = 2)
        {
            var records = new List<User>();
            for (int i = 1; i <= count; i++)
            {
                records.Add(new User
                {
                    Id = i,
                    DateCreated = currentTime,
                    FirstName = $"{i}-FirstName",
                    LastName = $"{i}-LastName",
                    Role = UserRole.UserManager,
                    EmailAddress = $"{i}_Email@mail.com",
                    UserName = $"{i}-Username",
                    Password = $"{i}-Password"
                });
            }

            return records;
        }
    }
}
