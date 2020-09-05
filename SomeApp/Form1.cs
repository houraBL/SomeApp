using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace SomeApp
{
    public partial class Form1 : Form
    {
        
        private Device device = null;
        private VertexBuffer gridVertexBuffer = null;
        private IndexBuffer gridIndexBuffer = null;

        private static int terWidth = 15;
        private static int terLength = 15;

        private float moveSpeed = 0.1f;
        private float turnSpeed = 0.05f;
        private float rotY = 0;
        private float tempY = 0;
        private float rotXZ = 0;
        private float tempXZ = 0;

        private static int vertCount = terWidth * terLength;
        private static int indCount = (terWidth - 1) * (terLength - 1) * 6;

        private Vector3 camPosition, camLookAt, camUp;

        CustomVertex.PositionColored[] verts = null;

        bool isMiddleMouseDown = false;

        private static int[] indices = null;

        private bool invalidating = true;

        public Form1()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);            
            InitializeComponent();
            InitializeGraphics();
            InitializeEventHandler();
        }

        private void InitializeGraphics()
        {
            PresentParameters pp = new PresentParameters();
            pp.Windowed = true;
            pp.SwapEffect = SwapEffect.Discard;

            pp.EnableAutoDepthStencil = true;

            pp.AutoDepthStencilFormat = DepthFormat.D16;

            device = new Device(0, DeviceType.Hardware, this, CreateFlags.HardwareVertexProcessing, pp);

            GenerateVertex();
            GenerateIndex();

            gridVertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), vertCount, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);
            OnVertexBufferCreate(gridVertexBuffer, null);

            gridIndexBuffer = new IndexBuffer(typeof(int), indCount, device, Usage.WriteOnly, Pool.Default);
            OnIndexBufferCreate(gridIndexBuffer, null);

            //Initial Camera Position
            camPosition = new Vector3(2, 4.5f, -3.5f);
            camUp = new Vector3(0, 1, 0);
        }

        private void InitializeEventHandler()
        {
            gridVertexBuffer.Created += new EventHandler(OnVertexBufferCreate);
            gridIndexBuffer.Created += new EventHandler(OnIndexBufferCreate);

            this.KeyDown += new KeyEventHandler(OnKeyDown);
            this.MouseWheel += new MouseEventHandler(OnMouseScroll);

            this.MouseMove += new MouseEventHandler(OnMouseMove);
            this.MouseDown += new MouseEventHandler(OnMouseDown);
            this.MouseUp += new MouseEventHandler(OnMouseUp);
        }

        private void OnIndexBufferCreate(object sender, EventArgs e)
        {
            IndexBuffer buffer = (IndexBuffer)sender;
            buffer.SetData(indices, 0, LockFlags.None);
        }

        private void OnVertexBufferCreate(object sender, EventArgs e)
        {
            VertexBuffer buffer = (VertexBuffer)sender;
            buffer.SetData(verts, 0, LockFlags.None);
        }

        private void SetupCamera()
        {
            camLookAt.X = (float)Math.Sin(rotY) + camPosition.X + (float)(Math.Sin(rotXZ) * Math.Sin(rotY));
            camLookAt.Y = (float)Math.Sin(rotXZ) + camPosition.Y;
            camLookAt.Z = (float)Math.Cos(rotY) + camPosition.Z + (float)(Math.Sin(rotXZ) * Math.Cos(rotY)); 

            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, this.Width / this.Height, 1.0f, 100.0f);
            device.Transform.View = Matrix.LookAtLH(camPosition, camLookAt, camUp);

            device.RenderState.Lighting = false;
            device.RenderState.CullMode = Cull.CounterClockwise;
            device.RenderState.FillMode = FillMode.WireFrame;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.CornflowerBlue, 1, 0);

            SetupCamera();
            device.BeginScene();

            device.VertexFormat = CustomVertex.PositionColored.Format; //check position normal colored
            device.SetStreamSource(0, gridVertexBuffer, 0);
            device.Indices = gridIndexBuffer;

            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertCount, 0, indCount / 3);

            device.EndScene();
            device.Present();
            menuStrip1.Update();
            if (invalidating)
            {
                this.Invalidate();
            }
        }

        // mesh generating VERTEX
        private void GenerateVertex()
        {
            verts = new CustomVertex.PositionColored[vertCount];
            int k = 0;

            for (int z = 0; z < terWidth; z++)
            {
                for (int x = 0; x < terLength; x++)
                {
                    verts[k].Position = new Vector3(x, 0, z);
                    verts[k].Color = Color.White.ToArgb();
                    k++;
                }
            }
        }

        // mesh generating INDEX
        private void GenerateIndex()
        {
            indices = new int[indCount];
            int k = 0;
            int l = 0;
            for (int i = 0; i < indCount; i += 6)
            {
                indices[i] = k;
                indices[i + 1] = k + terLength;
                indices[i + 2] = k + terLength + 1;
                indices[i + 3] = k;
                indices[i + 4] = k + terLength + 1;
                indices[i + 5] = k + 1;

                k++;
                l++;
                if (l == terLength - 1)
                {
                    l = 0;
                    k++;
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case (Keys.Up): //movw forward
                    {
                        camPosition.X += moveSpeed * (float)Math.Sin(rotY);
                        camPosition.Z += moveSpeed * (float)Math.Cos(rotY);
                        break;
                    }
                case (Keys.Down): //move back
                    {
                        camPosition.X -= moveSpeed * (float)Math.Sin(rotY);
                        camPosition.Z -= moveSpeed * (float)Math.Cos(rotY);
                        break;
                    }
                case (Keys.Left): // move left
                    {
                        camPosition.X -= moveSpeed * (float)Math.Sin(rotY + Math.PI / 2);
                        camPosition.Z -= moveSpeed * (float)Math.Cos(rotY + Math.PI / 2);
                        break;
                    }
                case (Keys.Right): // move right
                    {
                        camPosition.X += moveSpeed * (float)Math.Sin(rotY + Math.PI / 2);
                        camPosition.Z += moveSpeed * (float)Math.Cos(rotY + Math.PI / 2);
                        break;
                    }
                case (Keys.Oemcomma): // turn left "<" - button
                    {
                        rotY -= turnSpeed;
                        break;
                    }
                case (Keys.OemPeriod): // turn rigth ">" - button
                    {
                        rotY += turnSpeed;
                        break;
                    }
                case (Keys.OemCloseBrackets): // look up "{" button
                    {
                        if (rotXZ > - Math.PI / 2)
                        {
                            rotXZ -= turnSpeed;
                        }
                        break;
                    }
                case (Keys.OemOpenBrackets): // look down "}" button
                    {
                        if (rotXZ < Math.PI / 2)
                        {
                            rotXZ += turnSpeed;
                        }
                        break;
                    }
            }
        }

        private void OnMouseScroll(object sender, MouseEventArgs e)
        {
            camPosition.Y -= e.Delta * 0.001f;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isMiddleMouseDown)
            {
                rotY = tempY + e.X * turnSpeed;
                float tmp = tempXZ - e.Y * turnSpeed / 4;

                if (tmp < Math.PI / 2 && tmp > -Math.PI / 2)
                {
                    rotXZ = tmp;
                }
            }
        }
       
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case (MouseButtons.Middle):
                    {
                        tempY = rotY - e.X * turnSpeed;
                        tempXZ = rotXZ + e.Y * turnSpeed / 4;
                        isMiddleMouseDown = true;                        
                        break;
                    }
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case (MouseButtons.Middle):
                    {
                        tempY = rotY - e.X * turnSpeed;
                        isMiddleMouseDown = false;
                        break;
                    }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            invalidating = false;
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                //TODO open file and download all models
            }
            invalidating = true;
            this.Invalidate();
        }

        public class Model
        {
            public VertexBuffer Vertices { get; private set; }
            public IndexBuffer Indices { get; private set; }
        }
        /*
        public void RenderFrame(Model[] models)
        {
            // Per frame
            Bind(View);
            Bind(Projection);
            BindLighting();

            // Per effect
            BindEffect();

            foreach (var material in GetMaterials(models))
            {
                // Per material
                Bind(material.Color);
                Bind(material.DiffuseMap);

                foreach (var model in GetModelsByMaterial(material, models))
                {
                    // Per mesh
                    Bind(model.VertexBuffer);
                    Bind(model.IndexBuffer);

                    foreach (var instance in model.Instances)
                    {
                        // Per instance
                        Bind(instance.World);

                        // Draw the instance
                        Draw();
                    }
                }
            }
        }
        */
    }
}
