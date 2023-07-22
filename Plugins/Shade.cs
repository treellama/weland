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
    public static bool Compatible()
    {
        return true;
    }

    public static string Name()
    {
        return "Auto Shade Level";
    }

    const short Highlight = 3;
    const short Shadow = 13;
    const short Ceiling = 10;
    const short Floor = 5;

    static double Angle(Point p0, Point p1)
    {
        int X = p1.X - p0.X;
        int Y = p1.Y - p0.Y;
        double theta = Math.Atan2(-Y, X);
        if (theta < 0)
        {
            return -theta;
        }
        else
        {
            return theta;
        }
    }

    public static void Run(Editor editor)
    {
        Level level = editor.Level;

        for (int side_index = 0; side_index < level.Sides.Count; ++side_index)
        {
            Side side = level.Sides[side_index];
            Line line = level.Lines[side.LineIndex];
            double theta;
            if (line.ClockwisePolygonSideIndex == side_index)
            {
                theta = Angle(level.Endpoints[line.EndpointIndexes[0]], level.Endpoints[line.EndpointIndexes[1]]);
            }
            else
            {
                theta = Angle(level.Endpoints[line.EndpointIndexes[1]], level.Endpoints[line.EndpointIndexes[0]]);
            }

            short lightsource = (short)(Shadow + (Highlight - Shadow) * ((Math.Cos(theta) + 1) / 2));
            side.PrimaryLightsourceIndex = lightsource;
            side.SecondaryLightsourceIndex = lightsource;
        }

        foreach (Polygon polygon in level.Polygons)
        {
            polygon.FloorLight = Floor;
            polygon.CeilingLight = Ceiling;
        }
    }
}