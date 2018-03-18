﻿using System.Windows;

namespace SharpInjectorRework.Utilities
{
    internal class Messagebox
    {
        public static MessageBoxResult ShowError(string message, MessageBoxButton buttons = MessageBoxButton.OK)
            => MessageBox.Show(message, "Error", buttons, MessageBoxImage.Error);
        public static MessageBoxResult ShowInfo(string message, MessageBoxButton buttons = MessageBoxButton.OK)
            => MessageBox.Show(message, "Info", buttons, MessageBoxImage.Information);
        public static MessageBoxResult ShowWarning(string message, MessageBoxButton buttons = MessageBoxButton.OK)
            => MessageBox.Show(message, "Warning", buttons, MessageBoxImage.Warning);
    }
}