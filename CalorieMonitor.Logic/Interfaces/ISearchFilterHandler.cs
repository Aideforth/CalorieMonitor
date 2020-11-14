using CalorieMonitor.Core.Interfaces;
using System;

namespace CalorieMonitor.Logic.Interfaces
{
    public interface ISearchFilterHandler
    {
        IFilter Parse(string filter, Type type);
    }
}
