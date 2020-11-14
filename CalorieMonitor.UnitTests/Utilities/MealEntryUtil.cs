using CalorieMonitor.Core.Entities;
using System;
using System.Collections.Generic;
using Xunit;

namespace CalorieMonitor.UnitTests.Utilities
{
    public class MealEntryUtil
    {
        public static Action<MealEntry>[] GenerateVerifications(List<MealEntry> entries)
        {
            int count = entries.Count;
            Action<MealEntry>[] verifications = new Action<MealEntry>[count];
            for (int i = 1; i <= count; i++)
            {
                var entry = entries[i - 1];
                verifications[i - 1] = item =>
                {
                    Assert.Equal(entry.Id, item.Id);
                    Assert.Equal(entry.DateCreated, item.DateCreated);
                    Assert.Equal(entry.Calories, item.Calories);
                    Assert.Equal(entry.Text, item.Text);
                    Assert.Equal(entry.EntryDateTime, item.EntryDateTime);
                    Assert.NotNull(item.EntryCreator);
                    Assert.Equal(entry.EntryCreator.Id, item.EntryCreator.Id);
                    Assert.Equal(entry.EntryCreatorId, entry.EntryCreator.Id);
                    Assert.NotNull(item.EntryUser);
                    Assert.Equal(entry.EntryUser.Id, item.EntryUser.Id);
                    Assert.Equal(entry.EntryUserId, entry.EntryUser.Id);
                };
            }

            return verifications;
        }

        public static List<MealEntry> GenerateEntries(DateTime currentTime, int count = 2)
        {
            var entries = new List<MealEntry>();
            for (int i = 1; i <= count; i++)
            {
                entries.Add(new MealEntry
                {
                    Id = i,
                    EntryCreator = new User { Id = i },
                    EntryCreatorId = i,
                    DateCreated = currentTime,
                    Calories = 0,
                    Text = $"{i * 60}-Text",
                    EntryDateTime = currentTime,
                    EntryUser = new User { Id = i * 2 },
                    EntryUserId = i * 2
                });
            }

            return entries;
        }
    }
}
