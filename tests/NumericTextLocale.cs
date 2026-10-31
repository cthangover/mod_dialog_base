namespace Cthangover.Mods.Dialog.Tests
{
    using Cthangover.Core.Localization;
    internal class NumericTextLocale : ILocalizationProvider
    {
        public string Get(string key)=>key=="mod_dialog_base_name"?"Received ${money} coins.":null;
    }
}
