using System;
using System.Threading;

class Program
{
    // Define the vertices of the cube
    static readonly double[,] vertices = {
        {-1, -1, -1}, // 0
        {1, -1, -1},  // 1
        {1, 1, -1},   // 2
        {-1, 1, -1},  // 3
        {-1, -1, 1},  // 4
        {1, -1, 1},   // 5
        {1, 1, 1},    // 6
        {-1, 1, 1}    // 7
    };

    // Define the indices that make up each face of the cube
    static readonly int[,] indices = {
        {0, 1, 2, 3}, // front
        {1, 5, 6, 2}, // right
        {5, 4, 7, 6}, // back
        {4, 0, 3, 7}, // left
        {3, 2, 6, 7}, // top
        {4, 5, 1, 0}  // bottom
    };

    // Function to generate 3D points for the cube
    static double[,] GenerateCube()
    {
        double[,] points = new double[24, 3];
        int index = 0;

        // Loop through each face of the cube
        for (int face = 0; face < 6; face++)
        {
            // Loop through each vertex index of the face
            for (int i = 0; i < 4; i++)
            {
                // Add the vertex to the points array
                int vertexIndex = indices[face, i];
                points[index, 0] = vertices[vertexIndex, 0];
                points[index, 1] = vertices[vertexIndex, 1];
                points[index, 2] = vertices[vertexIndex, 2];
                index++;
            }
        }

        return points;
    }
    
    static void MultiplyMatrix(double[,] a, double[,] b, double[,] result)
    {
        for (int i = 0; i < a.GetLength(0); i++)
        {
            for (int j = 0; j < b.GetLength(1); j++)
            {
                double sum = 0;
                for (int k = 0; k < a.GetLength(1); k++)
                {
                    sum += a[i, k] * b[k, j];
                }
                result[i, j] = sum;
            }
        }
    }
    
    private static float angleX = 0;
    private static float angleY = 0;
    private static double[,] modelMatrix = new double[4, 4] {
        {1, 0, 0, 0},
        {0, 1, 0, 0},
        {0, 0, 1, 3},
        {0, 0, 0, 1}
    };

    static double[] TransformPoint(double[] point)
    {
        double x = point[0];
        double y = point[1];
        double z = point[2];

        // Create rotation matrices for X and Y axes
        double[,] rotationX = {
            {1, 0, 0, 0},
            {0, Math.Cos(angleX), -Math.Sin(angleX), 0},
            {0, Math.Sin(angleX), Math.Cos(angleX), 0},
            {0, 0, 0, 1}
        };

        double[,] rotationY = {
            {Math.Cos(angleY), 0, Math.Sin(angleY), 0},
            {0, 1, 0, 0},
            {-Math.Sin(angleY), 0, Math.Cos(angleY), 0},
            {0, 0, 0, 1}
        };

        // Multiply the rotation matrices to get the combined rotation
        double[,] rotationMatrix = new double[4, 4];
        MultiplyMatrix(rotationY, rotationX, rotationMatrix);

        // Apply the rotation matrix to the modelMatrix
        double[,] matrix = new double[4, 4];
        MultiplyMatrix(modelMatrix, rotationMatrix, matrix);

        // Apply the transformation matrix to the point
        double xNew = matrix[0, 0] * x + matrix[0, 1] * y + matrix[0, 2] * z + matrix[0, 3];
        double yNew = matrix[1, 0] * x + matrix[1, 1] * y + matrix[1, 2] * z + matrix[1, 3];
        double zNew = matrix[2, 0] * x + matrix[2, 1] * y + matrix[2, 2] * z + matrix[2, 3];

        return new double[] { xNew, yNew, zNew };
    }
    
    
    static double[] ProjectPoint(double[] point)
    {
        double x = point[0];
        double y = point[1];
        double z = point[2];
        double screenWidth = GetConsoleWidth();
        double screenHeight = GetConsoleHeight();

        double fovAngle = 90;
        double a = screenHeight / screenWidth;
        double f = 1 / Math.Tan(fovAngle / 2 * Math.PI / 180);

        double zFar = 1000;
        double zNear = 0.1;
        double lambda = zFar / (zFar - zNear);

        double newX = a * f * x;
        double newY = f * y;
        double newZ = z * lambda - zNear * lambda;

        // Perspective division
        if (newZ != 0)
        {
            newX = newX / newZ;
            newY = newY / newZ;
        }

        return new double[] { newX, newY };
    }

    static void UpdateCubeRotation()
    {
        angleX += 0.1f;
        angleY += 0.1f;
    }
    
    static void Main(string[] args)
    {
        Console.CursorVisible = false;

        while (true)
        {
            Console.Clear();
            UpdateCubeRotation();
            double[,] points = GenerateCube();
            double[][] transformedPoints = new double[24][];

            for (int i = 0; i < points.GetLength(0); i++)
            {
                double[] point = new double[] { points[i, 0], points[i, 1], points[i, 2] };
                double[] transformedPoint = TransformPoint(point);
                double[] projectedPoint = ProjectPoint(transformedPoint);

                int x = (int)(projectedPoint[0] * GetConsoleWidth() / 2 + GetConsoleWidth() / 2f);
                int y = (int)(projectedPoint[1] * GetConsoleHeight() / 2 + GetConsoleHeight() / 2f);
                transformedPoints[i] = new double[] { x, y };
            }

            for (int i = 0; i < transformedPoints.Length; i += 4)
            {
                DrawLine(transformedPoints[i], transformedPoints[i + 1]);
                DrawLine(transformedPoints[i + 1], transformedPoints[i + 2]);
                DrawLine(transformedPoints[i + 2], transformedPoints[i + 3]);
                DrawLine(transformedPoints[i + 3], transformedPoints[i]);
            }
            
            Thread.Sleep(50);
        }
    }
    
    static int GetConsoleWidth()
    {
        return Console.WindowWidth/2;
    }
    
    static int GetConsoleHeight()
    {
        return Console.WindowHeight;
    }
    
    static void DrawPixel(int x, int y)
    {
        if (x < 0 || x >= GetConsoleWidth() || y < 0 || y >= GetConsoleHeight())
        {
            return;
        }
        Console.SetCursorPosition(x*2, y);
        Console.Write("██");
    }

    static void DrawLine(double[] p1, double[] p2)
    {
        int x0 = (int)p1[0];
        int y0 = (int)p1[1];
        int x1 = (int)p2[0];
        int y1 = (int)p2[1];
        DrawPixel(x0, y0);
        double tDelta = 1f / Math.Max(Math.Abs(x1 - x0), Math.Abs(y1 - y0));
        for (double t = 0; t < 1; t += tDelta)
        {
            int x = (int)(x0 + (x1 - x0) * t);
            int y = (int)(y0 + (y1 - y0) * t);
            DrawPixel(x, y);
        }
        DrawPixel(x1, y1);
    }

}
