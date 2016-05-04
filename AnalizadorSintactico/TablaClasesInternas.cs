using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;


public class TablaClasesInternas
{
    public List<subTClase> tablaClases;


    public TablaClasesInternas()
    {
        tablaClases = new List<subTClase>();
    }

    public class subTClase
    {
        public string nombre;
        //public int nivel;
        public List<tipoVar> listaVar;

        public subTClase(string nombre1)
        {
            nombre = nombre1;
            //nivel = nivel1;
            listaVar = new List<tipoVar>();

        }

    }

    public class tipoVar
    {
        public string nombre;
        public string tipo;

        public tipoVar(string nombre1, string tip)
        {
            nombre = nombre1;
            tipo = tip;

        }

    }


    public void insertarClase(String nombre)
    {
        subTClase clase = new subTClase(nombre);
        tablaClases.Add(clase);

    }
    public void insertarVariable(String nombre, string tipo)
    {
        subTClase claseTemp = (subTClase)tablaClases.Last();
        tipoVar token = new tipoVar(nombre, tipo);
        claseTemp.listaVar.Add(token);
    }


    public tipoVar buscarPNombre(String nombre, String nombreClase)
    {
        subTClase claseTemp = (subTClase)tablaClases.Find(x => ((subTClase)x).nombre == nombreClase);
        tipoVar t = (tipoVar)claseTemp.listaVar.Find(x => ((tipoVar)x).nombre.Equals(nombre));
        return t;
    }

    public tipoVar buscarPNombreUltima(String nombre)
    {
        subTClase claseTemp = (subTClase)tablaClases.Last();
        tipoVar t = (tipoVar)claseTemp.listaVar.Find(x => ((tipoVar)x).nombre.Equals(nombre));
        return t;
    }



    public subTClase buscarCl(String nombreClase)
    {
        subTClase claseTemp = (subTClase)tablaClases.Find(x => ((subTClase)x).nombre == nombreClase);
        return claseTemp;
    }
    


}


