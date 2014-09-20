using System;

namespace iLynx.Common.WPF
{
    /// <summary>
    /// IGlobalHotkeyService
    /// </summary>
    public interface IGlobalHotkeyService
    {
        /// <summary>
        /// Registers the hotkey.
        /// </summary>
        /// <param name="hotkeyDescriptor">The hotkey registration.</param>
        /// <param name="callback">The callback.</param>
        void RegisterHotkey(HotkeyDescriptor hotkeyDescriptor, Action callback);

        /// <summary>
        /// Unregisters the hotkey.
        /// </summary>
        /// <param name="hotkeyDescriptor">The hotkey registration.</param>
        /// <param name="callback">The callback.</param>
        void UnregisterHotkey(HotkeyDescriptor hotkeyDescriptor, Action callback);

        /// <summary>
        /// Res the register hotkey.
        /// </summary>
        /// <param name="oldDescriptor">The old descriptor.</param>
        /// <param name="newDescriptor">The new descriptor.</param>
        /// <param name="callback">The callback.</param>
        void ReRegisterHotkey(HotkeyDescriptor oldDescriptor,
                              HotkeyDescriptor newDescriptor,
                              Action callback);
    }
}
