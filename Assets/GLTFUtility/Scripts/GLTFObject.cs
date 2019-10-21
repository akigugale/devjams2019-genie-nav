﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Siccity.GLTFUtility {
    public enum GLType { UNSET = -1, BYTE = 5120, UNSIGNED_BYTE = 5121, SHORT = 5122, UNSIGNED_SHORT = 5123, UNSIGNED_INT = 5125, FLOAT = 5126 }

    [Serializable]
    public class GLTFObject {

#region Serialized fields
        public int scene = -1;
        public List<GLTFScene> scenes;
        public List<GLTFNode> nodes;
        public List<GLTFMesh> meshes;
        public List<GLTFAnimation> animations;
        public List<GLTFBuffer> buffers;
        public List<GLTFBufferView> bufferViews;
        public List<GLTFAccessor> accessors;
        public List<GLTFSkin> skins;
        public List<GLTFTexture> textures;
        public List<GLTFImage> images;
        public List<GLTFMaterial> materials;
#endregion

#region Non-serialized fields
        public bool loaded { get; private set; }
        public string directoryRoot { get; private set; }
        public string mainFile { get; private set; }
#endregion

        public GameObject[] Create() {

            // Get root node indices from scenes
            int[] rootNodes = scenes.SelectMany(x => x.nodes).ToArray();

            GameObject[] roots = new GameObject[rootNodes.Length];
            for (int i = 0; i < rootNodes.Length; i++) {
                // Recursively construct transform hierarchy
                int nodeIndex = rootNodes[i];
                roots[i] = nodes[nodeIndex].CreateTransform(null).gameObject;
            }

            // Setup mesh renderers and such
            for (int i = 0; i < nodes.Count; i++) {
                nodes[i].SetupComponents();
            }
            return roots;
        }

        public void Load(string directoryRoot, string mainFile) {
            if (loaded) {
                Debug.LogWarning("GLTFObject already loaded");
                return;
            }
            this.directoryRoot = directoryRoot;
            this.mainFile = mainFile;
            Load(buffers);
            Load(bufferViews);
            Load(accessors);
            Load(images);
            Load(textures);
            Load(materials);
            Load(scenes);
            Load(nodes);
            Load(meshes);
            Load(animations);
            Load(skins);
            loaded = true;
        }

        private void Load<T>(List<T> properties) where T : GLTFProperty {
            for (int i = 0; i < properties.Count; i++) {
                properties[i].glTFObject = this;
            }
            for (int i = 0; i < properties.Count; i++) {
                properties[i].Load();
            }
        }
    }
}