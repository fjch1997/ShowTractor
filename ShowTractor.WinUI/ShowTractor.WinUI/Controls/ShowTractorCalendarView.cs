using Microsoft.UI.Xaml.Controls;
using System;

namespace ShowTractor.WinUI.Controls
{
    public class ShowTractorCalendarView : CalendarView
    {
        public Func<DateTimeOffset, object?>? DayItemDataContextProvider { get; set; }
        public ShowTractorCalendarView()
        {
            CalendarViewDayItemChanging += ShowTractorCalendarView_CalendarViewDayItemChanging;
        }
        private void ShowTractorCalendarView_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {
            if (DayItemDataContextProvider != null)
            {
                args.Item.DataContext = DayItemDataContextProvider(args.Item.Date);
            }
        }
    }
}
