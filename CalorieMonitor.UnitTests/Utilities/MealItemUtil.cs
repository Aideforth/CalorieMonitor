using CalorieMonitor.Core.Entities;
using System;
using System.Collections.Generic;
using Xunit;

namespace CalorieMonitor.UnitTests.Utilities
{
    public class MealItemUtil
    {
        public static Action<MealItem>[] GenerateVerifications(List<MealItem> items)
        {
            int count = items.Count;
            Action<MealItem>[] verifications = new Action<MealItem>[count];
            for (int i = 1; i <= count; i++)
            {
                var entry = items[i - 1];
                verifications[i - 1] = item =>
                {
                    Assert.Equal(entry.Id, item.Id);
                    Assert.Equal(entry.DateCreated, item.DateCreated);
                    Assert.Equal(entry.Calories, item.Calories);
                    Assert.Equal(entry.Name, item.Name);
                    Assert.Equal(entry.CaloriePerGram, item.CaloriePerGram);
                    Assert.Equal(entry.MealEntryId, item.MealEntryId);
                    Assert.Equal(entry.WeightInGrams, item.WeightInGrams);
                };
            }

            return verifications;
        }

        public static List<MealItem> GenerateItems(DateTime currentTime, int count = 2)
        {
            var items = new List<MealItem>();
            for (int i = 1; i <= count; i++)
            {
                items.Add(new MealItem
                {
                    Id = i,
                    DateCreated = currentTime,
                    Calories = i * 10,
                    Name = $"{i * 60}-Name",
                    CaloriePerGram = i * 0.2,
                    MealEntryId = i * 2,
                    WeightInGrams = i * 0.5
                });
            }

            return items;
        }
    }
}
