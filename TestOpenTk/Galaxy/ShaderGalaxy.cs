﻿using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTKUtils;
using OpenTKUtils.GL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOpenTk
{
    public class GalaxyFragmentPipeline : GLShaderPipelineShadersBase
    {
        string fcode = @"
#version 450 core
out vec4 color;

in vec3 vs_texcoord;

layout (binding=1) uniform sampler2D tex;
layout (binding=3) uniform sampler3D noise;     
layout (binding=4) uniform sampler1D gaussian;  

void main(void)
{
//color = texture(tex,vec2(vs_texcoord.x,vs_texcoord.z));     
//color = vec4(vs_texcoord.x, vs_texcoord.z,0,0);
//color.w = 0.5;
//color = vec4(vs_texcoord.x,vs_texcoord.y,vs_texcoord.z,1);

    float dx = abs(0.5-vs_texcoord.x);
    float dz = abs(0.5-vs_texcoord.z);
    float d = 0.7073-sqrt(dx*dx+dz*dz);     // 0 - 0.7
    d = d / 0.7073; // 0.707 is the unit circle, 1 is the max at corners

    if ( d > (1-0.707) )               // limit to circle around centre
    {
        vec4 c = texture(tex,vec2(vs_texcoord.x,vs_texcoord.z)); 
        float brightness = sqrt(c.x*c.x+c.y*c.y+c.z*c.z)/1.733;         // 0-1

        if ( brightness > 0.001 )
        {
            float g = texture(gaussian,d).x;      // look up single sided gaussian function, 0-1
            float h = abs(vs_texcoord.y-0.5)*2;     // 0-1 also

            if ( g > h )
            {
                float nv = texture(noise,vs_texcoord.xyz).x;
                float alpha = min(max(brightness,0.5),(brightness>0.05) ? 0.3 : brightness);    // beware the 8 bit alpha (0.0039 per bit).
                color = vec4(c.xyz*(1.0+nv*0.2),alpha*(1.0+nv*0.1));        // noise adjusting brightness and alpha a little
            }
            else 
                discard;
        }
        else
            discard;
    }
    else
        discard;

}
            ";

        public GalaxyFragmentPipeline()
        {
            CompileLink(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, fcode);
        }
    }

    public class GalaxyShader : GLShaderPipeline
    {
        public GalaxyShader()
        {
            Add(new GLPLVertexShaderVolumetric(), OpenTK.Graphics.OpenGL4.ShaderType.VertexShader);
            Add(new GLPLGeometricShaderVolumetric(), OpenTK.Graphics.OpenGL4.ShaderType.GeometryShader);
            Add(new GalaxyFragmentPipeline(), OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader);
        }
    }


    public class GalaxyStarDots : GLShaderStandard
    {
        string vert =
@"
        #version 450 core

        #include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl

        layout (location = 0) in vec4 position;     // has w=1
        out vec4 vs_color;

        void main(void)
        {
            vec4 p = position;
            p.w = 1;
            gl_Position = mc.ProjectionModelMatrix * p;        // order important
            vs_color = vec4(position.w,position.w,position.w,0.1);
        }
        ";
        string frag =
@"
        #version 450 core

        in vec4 vs_color;
        out vec4 color;

        void main(void)
        {
            color = vs_color;
        }
        ";
        public GalaxyStarDots() : base()
        {
            CompileLink(vert, frag: frag);
        }
    }


}