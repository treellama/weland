/*

	Copyright (C) 2011 Gregory Smith
 
	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	This license is contained in the file "COPYING",
	which is included with this source code; it is available online at
	http://www.gnu.org/licenses/gpl.html

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Weland;

public class ShadePlugin 
{
    public static bool IsCompatible => true;

    public static string Name => "Auto Shade Level";

    private const short Highlight = 3;
    private const short Shadow = 13;
    private const short Ceiling = 10;
    private const short Floor = 5;

    private static double Angle(Point p0, Point p1) 
    {
	var X = p1.X - p0.X;
	var Y = p1.Y - p0.Y;
	var theta = Math.Atan2(-Y, X);
	return Math.Abs(theta);
    }

    public static void Run(Editor editor)
    {
	var level = editor.Level;

	for (var side_index = 0; side_index < level.Sides.Count; ++side_index) 
	{
	    var side = level.Sides[side_index];
	    var line = level.Lines[side.LineIndex];
	    var theta = line.ClockwisePolygonSideIndex == side_index?
                        Angle(level.Endpoints[line.EndpointIndexes[0]], level.Endpoints[line.EndpointIndexes[1]]):
                        Angle(level.Endpoints[line.EndpointIndexes[1]], level.Endpoints[line.EndpointIndexes[0]]);
	    var lightsource = (short)(Shadow + (Highlight - Shadow) * ((Math.Cos(theta) + 1) / 2));
	    side.PrimaryLightsourceIndex = lightsource;
	    side.SecondaryLightsourceIndex = lightsource;
	}

	foreach (var polygon in level.Polygons) 
	{
	    polygon.FloorLight = Floor;
	    polygon.CeilingLight = Ceiling;
	}
    }
}
