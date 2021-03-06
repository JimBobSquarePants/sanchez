﻿using Sanchez.Extensions;
using Sanchez.Models;

namespace Sanchez.Factories
{
    internal static class RenderOptionFactory
    {
        /// <summary>
        ///     Creates render options from command line options.
        /// </summary>
        internal static RenderOptions ToRenderOptions(CommandLineOptions options)
        {
            return new RenderOptions(
                options.Brightness,
                options.Saturation,
                options.Tint.FromHexString());
        }
    }
}