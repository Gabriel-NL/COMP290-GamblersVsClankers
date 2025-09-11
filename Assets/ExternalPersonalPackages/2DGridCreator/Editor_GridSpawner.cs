using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static CustomUtils.GNLUtils;

[CustomEditor(typeof(MBH_GridSpawner))]
public class Editor_GridSpawner : Editor
{
    private static MBH_GridSpawner spawner;
    private SpriteRenderer[][] grid;
    private static Dictionary<int,UnityEngine.Color> color_dict=new Dictionary<int,UnityEngine.Color>();
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        spawner = (MBH_GridSpawner)target;

        if (GUILayout.Button("Create Grid"))
        {
            CreateGrid(spawner);
        }
    }

    void CreateGrid(MBH_GridSpawner spawner)
    {
        // Clean previous children
        for (int i = spawner.board_parent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(spawner.board_parent.GetChild(i).gameObject);
        }

        float totalSpacing = spawner.cellSize + spawner.gap_size;
        float spriteSize;
        int color_index=0;
        GameObject obj;
        SpriteRenderer spriteRenderer;

        for (int y = 0; y < spawner.height; y++)
        {
            for (int x = 0; x < spawner.width; x++)
            {
                obj=Instantiate(spawner.default_tile,spawner.board_parent);
                obj.name=$"Cell_{x}_{y}";
                obj.transform.localPosition = new Vector3(x * totalSpacing, y * totalSpacing, 0);
                
                spriteRenderer=obj.GetComponent<SpriteRenderer>();
                // Optional: scale sprite to cell size
                spriteSize = spriteRenderer.sprite.bounds.size.x;
                obj.transform.localScale = Vector3.one * (spawner.cellSize / spriteSize);

                if (color_dict.ContainsValue(spriteRenderer.color)==false)
                {
                    color_dict.Add(color_index++, spriteRenderer.color);
                }
            }
        }
    }    

    public void SetGridSprite(int x, int y,Sprite input_sprite){
        try
        {
            grid[x][y].sprite=input_sprite;
        }
        catch(System.IndexOutOfRangeException ioore){
           GNLPrintErr("Index outside of range. Here is the detail:",spawner.show_logs);
           GNLPrintErr(ioore.Message,spawner.show_logs);
        }
        catch(System.NullReferenceException NRE){
           GNLPrintErr("That object is null. Here is the detail:",spawner.show_logs);
           GNLPrintErr(NRE.Message,spawner.show_logs);
        }
        catch (System.Exception err)
        {
            GNLPrintErr("Uncaught error: "+err.Message,spawner.show_logs);
            throw;
        }
    }
    public Sprite GetGridSprite(int x, int y){
        try
        {
            return grid[x][y].sprite;
        }
        catch(System.IndexOutOfRangeException ioore){
           GNLPrintErr("Index outside of range. Here is the detail:",spawner.show_logs);
           GNLPrintErr(ioore.Message,spawner.show_logs);

        }
        catch(System.NullReferenceException NRE){
           GNLPrintErr("That object is null. Here is the detail:",spawner.show_logs);
           GNLPrintErr(NRE.Message,spawner.show_logs);
        }
        catch (System.Exception err)
        {
            GNLPrintErr("Uncaught error: "+err.Message,spawner.show_logs);
            throw;
        }
        return null;
    }
    
}
