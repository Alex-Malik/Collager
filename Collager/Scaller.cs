// This file is part of Collager.
// Copyright (C) 2016  Alex Malik
// 
// Collager is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Collager is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Collager.  If not, see<http://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collager
{
    using Behavior;
    using Models;

    public class Collager
    {
        public static ICollageBuildBehavior Builder { get; set; } = new Method2();

        public static IEnumerable<ImageRectangle> Create(IEnumerable<IImage> source)
        {
            return Builder.Build(source);
        }

        public static IEnumerable<ImageRectangle> Create(IEnumerable<IImage> source, double targetWidth, double targetHeight)
        {
            return Builder.Build(source, targetWidth, targetHeight);
        }
    }
    
    internal static class ListExtenstion
    {
        /// <summary>
        /// Returns a first element of the list and remove it from the list.
        /// </summary>
        public static T Pop<T>(this List<T> list)
        {
            T item = list.First();
            list.Remove(item);
            return item;
        }
    }
}
