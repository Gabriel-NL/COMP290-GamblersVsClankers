using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridBuilder<GenericType> where GenericType : UnityEngine.Component
{
    public Dictionary<(int,int) ,GenericType> customGrid;
    private Dictionary<float, int> xIndex ;
    private Dictionary<float, int> yIndex;


    public GridBuilder(Transform parent)
    {
        Transform[] children;
        if (parent.GetComponentsInChildren<Transform>()==null)
        {
            children = parent.GetComponentsInChildren<RectTransform>();
        }
        else
        {
            children = parent.GetComponentsInChildren<Transform>();
            
        }
        GenericType[] objects = parent.GetComponentsInChildren<GenericType>();
        InitializeEmptyGrid(children);
        AddValuesToGrid(objects);
    }
    public GridBuilder(Transform[] children)
    {
        InitializeEmptyGrid(children);

        foreach (var child in children)
        {
            var comp = child.GetComponent<GenericType>();
            if (comp == null) continue; // sem componente, sem problema

            var p = child.localPosition;
            int gx = xIndex[p.x];
            int gy = yIndex[p.y]; // troque para p.z se sua grade é XZ
            customGrid[(gx, gy)] = comp; // upsert direto
        }
    }

    public GridBuilder()
    {
        customGrid=new Dictionary<(int,int) ,GenericType>();
    }

    public void InitializeEmptyGrid(Transform[] childs)
    {
        List<Transform> children=childs.ToList();
        HashSet<float> xSet = new HashSet<float>();
        HashSet<float> ySet = new HashSet<float>();
        
        foreach (var child in children)
        {
            Vector3 p = child.localPosition;
            xSet.Add(p.x);
            ySet.Add(p.y);
        }
        List<float> xs = xSet.ToList();
        List<float> ys = ySet.ToList();
        xs.Sort();
        ys.Sort();
        xIndex = new Dictionary<float, int>();
        yIndex = new Dictionary<float, int>();
                
        for (int i = 0; i < xs.Count; i++)
        {
            xIndex[xs[i]] = i;
        }
        for (int i = 0; i < ys.Count; i++)
        {
            yIndex[ys[i]] = i;
        }
        customGrid=new Dictionary<(int,int) ,GenericType>();
        for (int i = 0; i < children.Count; i++)
        {
            Vector3 p = children[i].localPosition;
            int gx = xIndex[p.x];
            int gy = yIndex[p.y];
            if (customGrid.ContainsKey((gx, gy)))
            {
                GameObject.Destroy(children[i]);
            }
            else
            {
            customGrid.Add((gx,gy),default(GenericType) );
                
            }
        }
        
    }

    public void AddValueToGridCoordinates(GenericType type, int x, int y)
    {
        if (customGrid.ContainsValue(type))
        {
            customGrid.Add((x, y), type);
        }
    }
    public void AddValueToGrid(GenericType type,Transform typeTransform)
    {
        
        Vector3 p = typeTransform.localPosition;
        int gx = xIndex[p.x];
        int gy = yIndex[p.y];
        customGrid.Add((gx,gy) ,type);
    }
    public void AddValuesToGrid(GenericType[] type)
    {
        if (type==null || type.Length==0) {throw new ArgumentException("invalid type Array");}

        for (int i = 0; i < type.Length; i++)
        {
            Vector3 p = type[i].transform.localPosition;
            int gx = xIndex[p.x];
            int gy = yIndex[p.y];
            customGrid[(gx,gy)]= type[i];
        }
    }

}