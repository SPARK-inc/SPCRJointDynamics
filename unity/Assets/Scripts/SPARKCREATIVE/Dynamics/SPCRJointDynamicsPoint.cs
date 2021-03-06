﻿/*
 * MIT License
 *  Copyright (c) 2018 SPARKCREATIVE
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *  
 *  @author Noriyuki Hiromoto <hrmtnryk@sparkfx.jp>
*/

using UnityEngine;

[DisallowMultipleComponent]
public class SPCRJointDynamicsPoint : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private string uniqueGUIID;
    public string UniqueGUIID { get
        {
            if (string.IsNullOrEmpty(uniqueGUIID))
                GenerateNewID();
            return uniqueGUIID;
        }
    }

    [Header("=== 物理設定項目 ===")]
    public float _Mass = 1.0f;

    [Header("=== 物理自動設定項目 ===")]
    public SPCRJointDynamicsPoint _RefChildPoint;
    public bool _IsFixed;
    [HideInInspector]
    public Vector3 _BoneAxis = new Vector3(-1.0f, 0.0f, 0.0f);
    [HideInInspector]
    public float _Depth;
    [HideInInspector]
    public int _Index;

    public void Reset()
    {
        if (string.IsNullOrEmpty(UniqueGUIID))
            GenerateNewID();
    }

    void GenerateNewID()
    {
        uniqueGUIID = System.Guid.NewGuid().ToString();
    }
}
