using NUnit.Framework;
using NUnit.Framework.Legacy;
using ShowTractor.Interfaces;
using ShowTractor.Pages.Settings;
using ShowTractor.Plugins;
using ShowTractor.Tests.TestPlugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ShowTractor.Tests
{
    [TestFixture]
    public class PluginSettingsPageViewModelTests : IServiceProvider, IOpenFileDialogService
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private PluginSettingsPageViewModel subject;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private readonly PluginSettings settings = new();
        [TestCase]
        public async Task LoadNewPluginTest()
        {
            subject = new PluginSettingsPageViewModel(settings, this, this);
            var task = GetSettingsSavingTask();
            subject.LoadMetadataProviderCommand.Execute(null);
            await task;
            await AssertTestMetadataProviderAsync(settings);
            await TestRemove();
        }
        [TestCase]
        public async Task LoadSavedPluginTest()
        {
            settings.MetadataProviders.Add(new PluginDefinition { Enabled = true, FileName = await OpenFileAsync(Enumerable.Empty<string>()) });
            subject = new PluginSettingsPageViewModel(settings, this, this);
            await AssertTestMetadataProviderAsync(settings);
            await TestRemove();
        }
        private Task GetSettingsSavingTask()
        {
            var tcs = new TaskCompletionSource();
            if (!Debugger.IsAttached)
                _ = Task.Delay(1000).ContinueWith(t => tcs.TrySetCanceled()).ConfigureAwait(false);
            settings.SettingsSaving += (s, e) => { e.Cancel = true; tcs.TrySetResult(); };
            return tcs.Task;
        }
        private async Task TestRemove()
        {
            var task = GetSettingsSavingTask();
            subject.RemoveCommand.Execute(subject.MetadataProviders.First());
            await task;
            ClassicAssert.AreEqual(0, subject.MetadataProviders.Count);
            ClassicAssert.AreEqual(0, settings.MetadataProviders.Count);
        }
        private async Task AssertTestMetadataProviderAsync(PluginSettings settings)
        {
            ClassicAssert.AreEqual(1, settings.MetadataProviders.Count);
            ClassicAssert.AreEqual(await OpenFileAsync(Enumerable.Empty<string>()), settings.MetadataProviders[0].FileName);
            ClassicAssert.AreEqual(1, subject.MetadataProviders.Count);
            ClassicAssert.AreEqual(nameof(TestMetadataProvider), subject.MetadataProviders[0].Name);
            ClassicAssert.AreEqual(true, subject.MetadataProviders[0].Enabled);
        }
        public object? GetService(Type serviceType)
        {
            return null;
        }
        public Task<string?> OpenFileAsync(IEnumerable<string> filters)
        {
            return Task.FromResult<string?>(Assembly.GetExecutingAssembly().Location);
        }
    }
}
