using Newtonsoft.Json;
using System.Collections.Generic;

namespace CalorieMonitor.Logic.Implementations
{
    public class CalorieServiceResult
    {
        [JsonProperty("foods")]
        public List<CalorieServiceFood> Foods { get; set; }
    }
    public class CalorieServiceFood
    {
        [JsonProperty("food_name")]
        public string FoodName { get; set; }

        [JsonProperty("serving_weight_grams")]
        public double ServingWeightGrams { get; set; }

        [JsonProperty("nf_calories")]
        public double Calories { get; set; }
    }
}
