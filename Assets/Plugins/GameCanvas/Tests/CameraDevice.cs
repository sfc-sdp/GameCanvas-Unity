
namespace GameCanvas.Tests
{
    public sealed class CameraDevice : GameBase
    {
        private int mDeviceIndex;
        private string mDeviceName;

        public override void InitGame()
        {
            mDeviceIndex = 0;
            mDeviceName = gc.GetCameraDeviceName(mDeviceIndex);
            gc.StartCameraService(mDeviceIndex);
        }

        public override void UpdateGame()
        {
            if (gc.GetIsPointerEnded(0) && gc.GetPointerDuration(0) < 0.3f)
            {
                if (++mDeviceIndex >= gc.CameraDeviceCount) mDeviceIndex = 0;
                mDeviceName = gc.GetCameraDeviceName(mDeviceIndex);
                gc.StartCameraService(mDeviceIndex);
            }
        }

        public override void DrawGame()
        {
            gc.ClearScreen();
            gc.DrawScaledRotateCameraImage(100, 100, 25, 25, gc.CurrentCameraRotation);
            gc.DrawString(mDeviceName, 15, 15);
        }
    }
}
