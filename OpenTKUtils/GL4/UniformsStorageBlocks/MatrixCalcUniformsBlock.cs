﻿/*
 * Copyright © 2015 - 2018 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;

using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTKUtils.Common;

namespace OpenTKUtils.GL4
{
    // Class supports writing to local data then to the buffer object.


    // uniform blocks - std140 only
    public class GLMatrixCalcUniformBlock : GLUniformBlock 
    {
        public GLMatrixCalcUniformBlock() : base(0)         // 0 is the fixed binding block for matrixcalc
        {
        }

        public static string GLSL =
        @"
            layout(std140, binding=0) uniform MatrixCalc
            {
                mat4 ModelMatrix;
                mat4 ProjectionModelMatrix;
                mat4 InvEyeRotate;
                vec4 TargetPosition;
                vec4 EyePosition;
                float EyeDistance;
            } mc;
        ";

        public void Set( MatrixCalc c)
        {
            if (BufferSize == 0)
                Allocate(Mat4size * 3 + Vec4size * 2 + sizeof(float), BufferUsageHint.DynamicCopy);

            IntPtr ptr = Map(0, BufferSize);        // the whole schebang
            MapWrite(ref ptr, c.ModelMatrix);
            MapWrite(ref ptr, c.ProjectionModelMatrix);
            MapWrite(ref ptr, c.InvEyeRotate);
            MapWrite(ref ptr, c.TargetPosition,0);
            MapWrite(ref ptr, c.EyePosition, 0);
            MapWrite(ref ptr, c.EyeDistance);
            UnMap();                                // and complete..
        }

    }

}

