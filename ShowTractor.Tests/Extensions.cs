using NUnit.Framework;
using ShowTractor.Pages.Details;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ShowTractor.Tests
{
    public static class Extensions
    {
        public static async Task WaitForLoadingAsync(this LibraryViewModel libraryViewModel)
        {
            while (libraryViewModel.Loading)
            {
                await Task.Delay(0);
            }
        }
        public static async Task WaitForTrueAsync(this Func<bool> func)
        {
            var startTime = DateTime.UtcNow;
            while (!func())
            {
                if ((DateTime.UtcNow - startTime).TotalSeconds > 5 && !Debugger.IsAttached)
                    throw new AssertionException("Condition not satisfied in 5 seconds.");
                await Task.Delay(10);
            }
        }
    }
}
