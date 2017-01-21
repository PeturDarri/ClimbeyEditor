using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ClimbeyEditor
{
    public class TestScript : MonoBehaviour
    {
        public List<Camera> CameraViews;
        // Use this for initialization
        void Start()
        {
            var form = new MainForm();
            form.CameraViews = CameraViews;
            form.Show();
        }

        void OnGUI()
        {
            GUI.DrawTexture(new Rect(10, 0, 512, 512), CameraViews[0].targetTexture);
        }
    }
}