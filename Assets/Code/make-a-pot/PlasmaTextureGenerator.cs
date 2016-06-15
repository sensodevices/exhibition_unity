using System;
using UnityEngine;

public class PlasmaTextureGenerator {
    private int m_size;
    private int[] m_data;

    private int maxVal = 255;
    private System.Random rnd;

    public PlasmaTextureGenerator(int detail) {
        m_size = (1 << detail) + 1;
        m_data = new int[m_size * m_size];
        rnd = new System.Random();
    }

    public Texture2D Generate(int roughness, float roughnessDivider)
    {
        setVal(0, 0, maxVal * 2/3);
        setVal(m_size - 1, 0, maxVal / 2);
        setVal(m_size - 1, m_size - 1, maxVal / 2);
        setVal(0, m_size - 1, maxVal / 2);

        divide(m_size - 1, roughness, roughnessDivider);

        Texture2D aTexture = new Texture2D(m_size, m_size);
        for (var y = 0; y < m_size; ++y) {
            for (var x = 0; x < m_size; ++x) {
                float grColor = (getVal(x, y)) / 255.0f;
                Color color = new Color(1.0f, grColor, grColor, 1.0f);
                aTexture.SetPixel(x, y, color);
            }
        }
        aTexture.Apply();
        return aTexture;
    }

    private int getVal(int x, int y) {
        if (x < 0) x += (m_size - 1);
        else if (x >= m_size) x-= (m_size - 1);
        if (y < 0) y += (m_size - 1);
        else if (y >= m_size) y-= (m_size - 1);
        return m_data[x + m_size * y];
    }

    private void setVal(int x, int y, int val) {
        if (x < 0) x += (m_size - 1);
        else if (x >= m_size) x-= (m_size - 1);
        if (y < 0) y += (m_size - 1);
        else if (y >= m_size) y-= (m_size - 1);
        if (val > maxVal) val = maxVal;
        else if (val < 0) val = 0; 
        m_data[x + m_size * y] = val;
    }

    private void divide(int size, int amplitude, float roughnessDivider) {
        int x, y, half = size / 2;
        if (half < 1) return;

        for (y = half; y < m_size - 1; y += size) {
            for (x = half; x < m_size - 1; x += size) {
                square(x, y, half, rnd.Next(-amplitude, amplitude));
            }
        }
        for (y = 0; y <= m_size - 1; y += half) {
            for (x = (y + half) % size; x <= m_size - 1; x += size) {
              diamond(x, y, half, rnd.Next(-amplitude, amplitude));
            }
        }
        divide(half, (int)((float)amplitude / roughnessDivider), roughnessDivider);
    }

    private int average(int[] values) {
        int sum = 0;
        for (int i = 0; i < values.Length; ++i) {
            sum += values[i];
        }
        return sum / values.Length;
    }

    private void square(int x, int y, int size, int offset) {
        int[] values = new int[4] {
            getVal(x - size, y - size),   // upper left
            getVal(x + size, y - size),   // upper right
            getVal(x + size, y + size),   // lower right
            getVal(x - size, y + size)    // lower left
        };
        var ave = average(values);
        setVal(x, y, ave + offset);
    }

    private void diamond(int x, int y, int size, int offset) {
        int[] values = new int[4] {
            getVal(x, y - size),      // top
            getVal(x + size, y),      // right
            getVal(x, y + size),      // bottom
            getVal(x - size, y)       // left
        };
        var ave = average(values);
        setVal(x, y, ave + offset);
    }
}