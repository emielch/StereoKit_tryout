using StereoKit;
using StereoKit.Framework;
using System;

namespace StereoKitApp {
    public class App {
        public SKSettings Settings => new SKSettings {
            appName = "StereoKit Template",
            assetsFolder = "Assets",
            displayPreference = DisplayMode.MixedReality
        };

        Pose cubeAnchorPose = new Pose(0, 0, -0.5f, Quat.Identity);
        Model cube;
        Model cubeAnchor;

        Matrix floorTransform = Matrix.TS(new Vec3(0, -1.5f, 0), new Vec3(30, 0.1f, 30));
        Material floorMaterial;

        static Pose menuPose;
        bool showText = false;
        static Pose textPose = new Pose(0f, 0, -0.4f, Quat.LookDir(-1, 0, 1));

        bool showCube = false;
        bool showAnchor = false;
        bool movingCube = false;

        public void PreInit() {
            SK.AddStepper<PassthroughFBExt>();
        }
        
        public void Init() {
            menuPose = GetUserFrontPose();
            cube = Model.FromMesh(
                Mesh.GenerateRoundedCube(Vec3.One * 0.1f, 0.02f),
                Default.MaterialUI);
            cubeAnchor = Model.FromMesh(
                Mesh.GenerateRoundedCube(Vec3.One * 0.05f, 0.02f),
                Default.MaterialUI);

            floorMaterial = new Material(Shader.FromFile("floor.hlsl"));
            floorMaterial.Transparency = Transparency.Blend;
        }

        public void Step() {
            if (SK.System.displayType == Display.Opaque)
                Default.MeshCube.Draw(floorMaterial, floorTransform);

            if (showCube) {
                Pose cubePose = cubeAnchorPose;
                if (movingCube) cubePose.position += new Vec3((float)Math.Sin(Time.Total * 2f) * 0.2f, (float)Math.Sin(Time.Total * 4f) * 0.1f, (float)Math.Sin(Time.Total) * 0.05f);

                if (showAnchor) {
                    if (movingCube) {
                        UI.Handle("Cube", ref cubeAnchorPose, cubeAnchor.Bounds);
                        cubeAnchor.Draw(cubeAnchorPose.ToMatrix());
                    } else {
                        UI.Handle("Cube", ref cubeAnchorPose, cube.Bounds);
                    }
                }

                cube.Draw(cubePose.ToMatrix());
            }

            StepMenuWindow();
            if (showText) StepText();
        }

        void StepText() {
            textPose = Pose.Lerp(textPose, GetUserFrontPose(), 0.05f);
            Text.Add("Hello World", textPose.ToMatrix());
        }

        Pose GetUserFrontPose() {
            Vec3 textPos = Input.Head.position + Input.Head.Forward * 0.4f;
            Quat textRot = Quat.LookAt(textPos, Input.Head.position);
            return new Pose(textPos, textRot);
        }

        void SpawnCube() {
            showCube = true;
            cubeAnchorPose = GetUserFrontPose();
        }

        void AnchorButtonPressed() {
            if (!showCube) {
                SpawnCube();
                movingCube = false;
                showAnchor = true;
                return;
            }
            showAnchor = !showAnchor;
        }

        void CubeButtonPressed() {
            showCube = !showCube;
            if (showCube) {
                SpawnCube();
                movingCube = true;
                showAnchor = false;
            }
        }

        void StepMenuWindow() {
            UI.WindowBegin("Menu", ref menuPose, UIWin.Body);

            if (UI.Toggle("Text", ref showText)) {
                textPose = GetUserFrontPose();
            }
            if (UI.Button("Cube")) CubeButtonPressed();
            if (UI.Button("Handle")) AnchorButtonPressed();

            UI.WindowEnd();
        }

    }
}