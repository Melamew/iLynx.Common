using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace iLynx.Common.WPF.Imaging
{
    public static class Extensions
    {
        public static unsafe void DrawLine(this IntPtr target,
            int width,
            int height,
            int x1,
            int y1,
            int x2,
            int y2,
            int colour)
        {
            // Use refs for faster access (really important!) speeds up a lot!
            var w = width;
            var h = height;
            var pixels = (int*) target;

            // Distance start and end point
            var dx = x2 - x1;
            var dy = y2 - y1;

            // Determine sign for direction x
            var incx = 0;
            if (dx < 0)
            {
                dx = -dx;
                incx = -1;
            }
            else if (dx > 0)
            {
                incx = 1;
            }

            // Determine sign for direction y
            var incy = 0;
            if (dy < 0)
            {
                dy = -dy;
                incy = -1;
            }
            else if (dy > 0)
            {
                incy = 1;
            }

            // Which gradient is larger
            int pdx, pdy, odx, ody, es, el;
            if (dx > dy)
            {
                pdx = incx;
                pdy = 0;
                odx = incx;
                ody = incy;
                es = dy;
                el = dx;
            }
            else
            {
                pdx = 0;
                pdy = incy;
                odx = incx;
                ody = incy;
                es = dx;
                el = dy;
            }

            // Init start
            var x = x1;
            var y = y1;
            var error = el >> 1;
            if (y < h && y >= 0 && x < w && x >= 0)
            {
                pixels[y*w + x] = colour;
            }

            // Walk the line!
            for (var i = 0; i < el; i++)
            {
                // Update error term
                error -= es;

                // Decide which coord to use
                if (error < 0)
                {
                    error += el;
                    x += odx;
                    y += ody;
                }
                else
                {
                    x += pdx;
                    y += pdy;
                }

                // Set pixel
                if (y < h && y >= 0 && x < w && x >= 0)
                {
                    pixels[y*w + x] = colour;
                }
            }
        }

        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void DrawLineVector(this IntPtr target,
            VectorD start,
            VectorD stop,
            ColorForPixel colorForPixel,
            int width,
            int height)
        {
            var buf = (int*) target;
            var lineVector = stop - start;
            var steps = (int) Math.Max(Math.Abs(lineVector.X), Math.Abs(lineVector.Y));
            var uX = lineVector.X/steps;
            var uY = lineVector.Y/steps;
            Parallel.For(0, steps, i =>
                                   {
                                       var x = start.X + uX*i;
                                       var y = start.Y + uY*i;
                                       if (y > -1 && x > -1 && x < width && y < height)
                                           buf[(int) y*width + (int) x] = colorForPixel(x, y, i/(double) steps);
                                   }
                );
        }

        [TargetedPatchingOptOut("Performance Critical")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DrawLineVector(this IntPtr target, VectorD start, VectorD stop, Color colour, int width, int height)
        {
            var col = colour.Intify();
            target.DrawLineVector(start, stop, (d, d1, position) => col, width, height);
        }

        [TargetedPatchingOptOut("Performance")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DrawLineVector(this IntPtr target,
            VectorD start,
            VectorD stop,
            Color startColor,
            Color stopColor,
            int width,
            int height)
        {
            var rFac = (stopColor.R - startColor.R);
            var gFac = (stopColor.G - startColor.G);
            var bFac = (stopColor.B - startColor.B);
            var aFac = (stopColor.A - startColor.A);
            target.DrawLineVector(start, stop, (d, d1, position) => Color.FromArgb((byte) (startColor.A + aFac*position),
                (byte) (startColor.R + rFac*position),
                (byte) (startColor.G + gFac*position),
                (byte) (startColor.B + bFac*position)).Intify(), width,
                height);
        }

        [TargetedPatchingOptOut("Wrapper")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HitTest(VectorD vector, int width, int height)
        {
            return vector.X >= 0 && vector.Y >= 0 && vector.X < width && vector.Y < height;
        }

        [TargetedPatchingOptOut("Wrapper")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HitTest(double x,
            double y,
            int width,
            int height)
        {
            return x > -1 && y > -1 && x < width && y < height;
        }

        [TargetedPatchingOptOut("Performance")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Intify(this Color color)
        {
            return unchecked(color.A << 24 | color.R << 16 | color.G << 8 | color.B);
        }

        [TargetedPatchingOptOut("Performance")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Colorify(this int integer)
        {
            return Color.FromArgb(
                (byte) ((integer >> 24) & 0xFF),
                (byte) ((integer >> 16) & 0xFF),
                (byte) ((integer >> 8)  & 0xFF),
                (byte) (integer & 0xFF)
                );
        }

        public static void DrawLine(this RenderContext context, Point p1, Point p2, Color color)
        {
            context.DrawLine(p1, p2, color.Intify());
        }

        [TargetedPatchingOptOut("Wrapper")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DrawLine(this RenderContext context,
            Point p1,
            Point p2,
            int color)
        {
            context.BackBuffer.DrawLine(context.Width, context.Height, (int) p1.X, (int) p1.Y, (int) p2.X, (int) p2.Y, color);
        }

        [TargetedPatchingOptOut("Wrapper")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DrawEllipse(this RenderContext context, VectorD center, VectorD radius, Color color)
        {
            context.BackBuffer.DrawEllipse((int) center.X, (int) center.Y, (int) radius.X, (int) radius.Y, color.Intify(), context.Width,
                context.Height);
        }

        [TargetedPatchingOptOut("Performance")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void DrawEllipse(this IntPtr backBuffer, int xc, int yc, int xr, int yr, int color, int w, int h)
        {
            var pixels = (int*) backBuffer;
            // Avoid endless loop
            if (xr < 1 || yr < 1)
            {
                return;
            }

            // Init vars
            int uh, lh, uy, ly, lx, rx;
            var x = xr;
            var y = 0;
            var xrSqTwo = (xr*xr) << 1;
            var yrSqTwo = (yr*yr) << 1;
            var xChg = yr*yr*(1 - (xr << 1));
            var yChg = xr*xr;
            var err = 0;
            var xStopping = yrSqTwo*xr;
            var yStopping = 0;

            // Draw first set of points counter clockwise where tangent line slope > -1.
            while (xStopping >= yStopping)
            {
                // Draw 4 quadrant points at once
                uy = yc + y; // Upper half
                ly = yc - y; // Lower half
                if (uy < 0) uy = 0; // Clip
                if (uy >= h) uy = h - 1; // ...
                if (ly < 0) ly = 0;
                if (ly >= h) ly = h - 1;
                uh = uy*w; // Upper half
                lh = ly*w; // Lower half

                rx = xc + x;
                lx = xc - x;
                if (rx < 0) rx = 0; // Clip
                if (rx >= w) rx = w - 1; // ...
                if (lx < 0) lx = 0;
                if (lx >= w) lx = w - 1;
                pixels[rx + uh] = color; // Quadrant I (Actually an octant)
                pixels[lx + uh] = color; // Quadrant II
                pixels[lx + lh] = color; // Quadrant III
                pixels[rx + lh] = color; // Quadrant IV

                y++;
                yStopping += xrSqTwo;
                err += yChg;
                yChg += xrSqTwo;
                if ((xChg + (err << 1)) > 0)
                {
                    x--;
                    xStopping -= yrSqTwo;
                    err += xChg;
                    xChg += yrSqTwo;
                }
            }

            // ReInit vars
            x = 0;
            y = yr;
            uy = yc + y; // Upper half
            ly = yc - y; // Lower half
            if (uy < 0) uy = 0; // Clip
            if (uy >= h) uy = h - 1; // ...
            if (ly < 0) ly = 0;
            if (ly >= h) ly = h - 1;
            uh = uy*w; // Upper half
            lh = ly*w; // Lower half
            xChg = yr*yr;
            yChg = xr*xr*(1 - (yr << 1));
            err = 0;
            xStopping = 0;
            yStopping = xrSqTwo*yr;

            // Draw second set of points clockwise where tangent line slope < -1.
            while (xStopping <= yStopping)
            {
                // Draw 4 quadrant points at once
                rx = xc + x;
                lx = xc - x;
                if (rx < 0) rx = 0; // Clip
                if (rx >= w) rx = w - 1; // ...
                if (lx < 0) lx = 0;
                if (lx >= w) lx = w - 1;
                pixels[rx + uh] = color; // Quadrant I (Actually an octant)
                pixels[lx + uh] = color; // Quadrant II
                pixels[lx + lh] = color; // Quadrant III
                pixels[rx + lh] = color; // Quadrant IV

                x++;
                xStopping += yrSqTwo;
                err += xChg;
                xChg += yrSqTwo;
                if ((yChg + (err << 1)) <= 0) continue;
                y--;
                uy = yc + y; // Upper half
                ly = yc - y; // Lower half
                if (uy < 0) uy = 0; // Clip
                if (uy >= h) uy = h - 1; // ...
                if (ly < 0) ly = 0;
                if (ly >= h) ly = h - 1;
                uh = uy*w; // Upper half
                lh = ly*w; // Lower half
                yStopping -= xrSqTwo;
                err += yChg;
                yChg += xrSqTwo;
            }
        }

        public static unsafe void FillPolygon(this IntPtr backBuffer, int width, int height, int[] points, int color)
        {
            var pn = points.Length;
            var pnh = points.Length >> 1;
            var intersectionsX = new int[pnh];
            var pixels = (int*) backBuffer;

            // Find y min and max (slightly faster than scanning from 0 to height)
            var yMin = height;
            var yMax = 0;
            for (var i = 1; i < pn; i += 2)
            {
                var py = points[i];
                if (py < yMin) yMin = py;
                if (py > yMax) yMax = py;
            }
            if (yMin < 0) yMin = 0;
            if (yMax >= height) yMax = height - 1;


            // Scan line from min to max
            for (var y = yMin; y <= yMax; y++)
            {
                // Initial point x, y
                float vxi = points[0];
                float vyi = points[1];

                // Find all intersections
                // Based on http://alienryderflex.com/polygon_fill/
                var intersectionCount = 0;
                for (var i = 2; i < pn; i += 2)
                {
                    // Next point x, y
                    float vxj = points[i];
                    float vyj = points[i + 1];

                    // Is the scanline between the two points
                    if (vyi < y && vyj >= y
                        || vyj < y && vyi >= y)
                    {
                        // Compute the intersection of the scanline with the edge (line between two points)
                        intersectionsX[intersectionCount++] = (int) (vxi + (y - vyi)/(vyj - vyi)*(vxj - vxi));
                    }
                    vxi = vxj;
                    vyi = vyj;
                }

                // Sort the intersections from left to right using Insertion sort 
                // It's faster than Array.Sort for this small data set
                for (var i = 1; i < intersectionCount; i++)
                {
                    var t = intersectionsX[i];
                    var j = i;
                    while (j > 0 && intersectionsX[j - 1] > t)
                    {
                        intersectionsX[j] = intersectionsX[j - 1];
                        j = j - 1;
                    }
                    intersectionsX[j] = t;
                }

                // Fill the pixels between the intersections
                for (var i = 0; i < intersectionCount - 1; i += 2)
                {
                    var x0 = intersectionsX[i];
                    var x1 = intersectionsX[i + 1];

                    // Check boundary
                    if (x1 <= 0 || x0 >= width) continue;
                    if (x0 < 0) x0 = 0;
                    if (x1 >= width) x1 = width - 1;

                    // Fill the pixels
                    for (var x = x0; x <= x1; x++)
                    {
                        pixels[y*width + x] = color;
                    }
                }
            }
        }

        public static void FillQuad(this IntPtr bmp, int width, int height, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, Color color)
        {
            var col = color.Intify();
            bmp.FillQuad(width, height, x1, y1, x2, y2, x3, y3, x4, y4, col);
        }

        public static void FillQuad(this IntPtr backBuffer, int width, int height, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, int color)
        {
            backBuffer.FillPolygon(width, height, new[] { x1, y1, x2, y2, x3, y3, x4, y4, x1, y1 }, color);
        }
    }
}