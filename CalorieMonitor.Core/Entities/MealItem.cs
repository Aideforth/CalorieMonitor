namespace CalorieMonitor.Core.Entities
{
    public class MealItem : Entity
    {
        public string Name { get; set; }
        public double Calories { get; set; }
        public double WeightInGrams { get; set; }
        public double CaloriePerGram { get; set; }
        public long MealEntryId { get; set; }
    }
}
