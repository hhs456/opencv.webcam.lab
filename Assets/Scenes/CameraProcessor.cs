using OpenCvSharp;
using OpenCvSharp.Demo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraProcessor : WebCamera
{
    [SerializeField] private FlipMode imageFlip;
    [SerializeField] private float threshould = 96.4f;
    [SerializeField] private bool enableProcessing = true;
    [SerializeField] private float curveAccuracy = 10f;
    [SerializeField] private float mainArea = 5000f;
    [SerializeField] private PolygonCollider2D polygonCollider;
    private Mat image;
    private Mat processedImage = new Mat();
    private Point[][] contours;
    private HierarchyIndex[] hierarchy;
    private Vector2[] vectorArray;

    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output) {
        image = OpenCvSharp.Unity.TextureToMat(input);
        Cv2.Flip(image, image, imageFlip);
        Cv2.CvtColor(image, processedImage, ColorConversionCodes.BGR2GRAY);
        Cv2.Threshold(processedImage, processedImage, threshould, 255, ThresholdTypes.BinaryInv);
        Cv2.FindContours(processedImage, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple, null);

        polygonCollider.pathCount = 0;
        foreach (var contour in contours) {
            Point[] points = Cv2.ApproxPolyDP(contour, curveAccuracy, true);
            var area = Cv2.ContourArea(contour);

            if(area > mainArea) {
                DrawContour(processedImage, new Scalar(127, 127, 127), 2, points);
                polygonCollider.pathCount++;
                polygonCollider.SetPath(polygonCollider.pathCount - 1, ToVector2(points));
            }
        }

        if(output == null) {
            output = OpenCvSharp.Unity.MatToTexture(enableProcessing ? processedImage : image);
        }
        else {
            OpenCvSharp.Unity.MatToTexture(enableProcessing ? processedImage : image, output);
        }
        return true;
    }    

    private void DrawContour(Mat image, Scalar color, int thickness, Point[] points) {
        for (int i = 1; i< points.Length; i++) {
            Cv2.Line(image, points[i - 1], points[i], color, thickness);
        }
        Cv2.Line(image, points[points.Length - 1], points[0], color, thickness);
    }

    private Vector2[] ToVector2(Point[] points) {
        vectorArray = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++) {
            vectorArray[i] = new Vector2(points[i].X, points[i].Y);
        }
        return vectorArray;
    }
}
