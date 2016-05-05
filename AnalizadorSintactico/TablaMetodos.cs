using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TablaMetodos
{

     List<ElementoMet> listaMetodos;

    public TablaMetodos()
    {
        listaMetodos = new List<ElementoMet>();
    }

    public class ElementoMet {
        public String nombre;
        public String tipo;
        public int numPara;
        public String[] tiposPara;

        public ElementoMet(String nom, String tip, int num, String[] tiposP)
        {
            nombre = nom;
            tipo = tip;
            numPara = num;
            tiposPara = tiposP;
        }

  
    }



    public void insertar(String nombre, String tipo, String[] tiposPar)
    {
        ElementoMet token = new ElementoMet(nombre, tipo, tiposPar.Length, tiposPar);

        listaMetodos.Add(token);
    }
   

    public ElementoMet buscarPNombre(String nombre)
    {
        ElementoMet elemP = (ElementoMet)listaMetodos.Find(x => ((ElementoMet)x).nombre == nombre);
        return elemP;
    }


    public String ultimoTipo()
    {
        ElementoMet claseTemp = (ElementoMet)listaMetodos.Last();
        return claseTemp.tipo;
    }
  

}


