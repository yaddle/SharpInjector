﻿using System.Linq;
using System.Diagnostics;

namespace SharpInjectorRework.Utilities
{
    internal static class Process
    {
        public static bool IsProcessValid(System.Diagnostics.Process process)
            => process != null && !process.HasExited;

        public static System.Diagnostics.Process[] FilterValidProcesses(System.Diagnostics.Process[] process_list)
            => process_list.Where(IsProcessValid).ToArray();

        public static bool GetModule(System.Diagnostics.Process process, string module_name, out ProcessModule module_handle)
        {
            module_handle = null;

            if (!IsProcessValid(process))
            {
                Utilities.Messagebox.ShowError($"failed to get module '{module_name}', invalid process");
                return false;
            }

            foreach (ProcessModule process_module in process.Modules)
            {
                if (process_module == null)
                    continue;

                var module_name_lower = module_name.ToLower();
                var process_module_name_lower = process_module.ModuleName.ToLower();

                if (!process_module_name_lower.Equals(module_name_lower))
                    continue;

                module_handle = process_module;

                return true;
            }

            Utilities.Messagebox.ShowError($"failed to get module '{module_name}', module was not found");

            return false;
        }
    }
}