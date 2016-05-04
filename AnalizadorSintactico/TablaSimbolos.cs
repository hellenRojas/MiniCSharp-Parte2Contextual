using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;


/*
 Tipos:
 * 0 : undefined
 * 1 : int
 * 2 : char
 * 3 : float
 * 4 : int []
 * 5 : char []
 * 6 : float []
 * 8 : boolean
 * 9 : void
 * 10: class
 * 11: metodo
  clases:
 * var : variables
 * param : parametros
 * const : constantes
 
 */

class TablaSimbolos
{

    List<ElementoG> listVar;

    public TablaSimbolos()
    {

        listVar = new List<ElementoG>();
    }

    public class ElementoG
    {
        public string nombre;
        public int nivel;
        public string tipo;
        public string claseEl;

        public ElementoG(string nom, int niv, string tp, string cl)
        {
            nombre = nom;
            nivel = niv;
            tipo = tp;
            claseEl = cl;
        }


    }


    //nivel,nombre, tipo
    public void insertar(String nombre, int nivel, string tipo, string cl)
    {

        ElementoG e = new ElementoG(nombre, nivel, tipo, cl);

        listVar.Add(e);
    }

    public ElementoG buscar(String nombre)
    {
        return (ElementoG)listVar.Find(item => ((ElementoG)item).nombre.Equals(nombre));
    }
    public void cerrarNivel()
    {
        ElementoG[] listaTemp= new ElementoG[listVar.Count];
        listaTemp = listVar.ToArray();
        foreach (ElementoG elem in listaTemp)
        {
            if (elem.nivel == 1) 
            {
                EliminarVar(elem);
            }
        }
           

    }
    private void EliminarVar(ElementoG elem)
    {
        listVar.Remove(elem);
    }

}


