using System;
using UnityEngine;


[ExecuteInEditMode]
public class MBH_GridSpawner : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    public float cellSize = 1f;
    public float gap_size = 0;
    public Transform board_parent;
    public GameObject default_tile;

    public bool show_logs=false;
}
