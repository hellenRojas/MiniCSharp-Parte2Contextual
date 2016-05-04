using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using AnalizadorSintactico;
using System.Collections;

//REVISAR EL TYPE

// Tiene errores
class AContextual : Parser1BaseVisitor<Object>
{
    public TablaSimbolos table; // Tabla general
    public TablaClasesInternas tableClases; // Tabla de clases
    public TablaMetodos tableMethods; // Tabla de metodos
    int tablaActual = 0; // si es 1 se debe insertar en la tabla de clases
    public int nivelActual = 0;
    string nombreMetodo_Llamada =""; // variable que guarda el nombre del metodo actual; se necesita en el designator
    bool tRetorno = true;
    bool retornoFun = false;
    string banderaClaseVar = "";
    public string msgError = "";
    public int lineaActual = 0;
    public bool esNew = false;
    public AContextual()
    {
        table = new TablaSimbolos();
        tableClases = new TablaClasesInternas();
        tableMethods = new TablaMetodos();


    }



    //************************************************  GENERALES *************************************************************

    public override object VisitProgramAST([NotNull] Parser1.ProgramASTContext context)
    {
        string [] tipos = { "array[]"};
        string [] tipCH = {"int"};
        string[] tipOrd = { "char" };
        tableMethods.insertar("len","int",tipos);
        tableMethods.insertar("chr", "char", tipCH);
        tableMethods.insertar("ord", "int", tipOrd);
        table.insertar("null",0,"null","");
        if (context.classDecl() != null)
        {
            for (int i = 0; i < context.classDecl().Count(); i++)
            {
                Visit(context.classDecl(i));
            }
        }
        if (context.constDecl() != null)
        {
            for (int i = 0; i < context.constDecl().Count(); i++)
            {
                Visit(context.constDecl(i));
            }
        }
        if (context.varDecl() != null)
        {
            for (int i = 0; i < context.varDecl().Count(); i++)
            {
                Visit(context.varDecl(i));
            }
        }
        
        for (int i = 0; i < context.methodDecl().Count(); i++)
        {
            Visit(context.methodDecl(i));
        }

        return null; //en caso de ERROR cambiar a null



    }
    //NO HAY PROBLEMA DE ERRORES
    public override object VisitConstDeclAST([NotNull] Parser1.ConstDeclASTContext context)
    {
        string tipoC1;
        context.CONSTANTE();
        try
        {
             tipoC1 = (string)Visit(context.type());
        
            if (tipoC1 != "int" && tipoC1 != "char")
            {
                Console.WriteLine("Tipo no permitido: " + tipoC1);
                 msgError = msgError + "Linea: " + context.CONSTANTE().Symbol.Line + "-> Tipo no permitido: " + tipoC1 + "\n";
                 return null;
            }
            string id = context.ID().GetText();
            
            table.insertar(id, nivelActual, tipoC1, "const");
            string tipoC2 = null;
            if (context.NUMBER() != null)
            {
                tipoC2 = "int";
            }
            else if (context.CharConst() != null)
            {
                tipoC2 = "char";
            }
            /*
             if (tipoC2 == null)
             {
                 Console.WriteLine("Tipo no permitido debe ser de tipo int o char");
                 return null;
             }
             */
            if (tipoC1 != tipoC2)
            {
                msgError = msgError + "Linea: " + context.CONSTANTE().Symbol.Line + "-> Tipos Incompatibles (" + tipoC1 + "," + tipoC2 + ") \n";
            }


            
        }
           catch(Exception e ){
            Console.WriteLine(e.Message);
            msgError = msgError + e.Message + "\n";
        };

        return null;
       

    }

    //NO HAY PROBLEMA DE ERRORES
    public override object VisitVarDeclAST([NotNull] Parser1.VarDeclASTContext context)
    {
        string tipo;
        try{
             tipo = (string)Visit(context.type());

             if (tablaActual == 1)
             {
                 for (int i = 0; i <= context.ID().Length - 1; i++)
                 {


                     if (tableClases.buscarPNombreUltima(context.ID(i).GetText()) == null)
                     {
                         tableClases.insertarVariable(context.ID(i).GetText(),tipo);
                     }
                     else
                     {
                        throw new Exception("Linea: "+ context.ID(i).Symbol.Line+"-> La variable " + context.ID(i).GetText() + " ya esta definida");
              
                     }
                 }

               
             }
             else
             {

                 for (int i = 0; i <= context.ID().Length - 1; i++)
                 {
                     if (table.buscar(context.ID(i).GetText()) == null)
                     {
                         table.insertar(context.ID(i).GetText(), nivelActual, tipo, "var");
                     }
                     else
                     {
                         throw new Exception("Linea: " + context.ID(i).Symbol.Line + "-> La variable " + context.ID(i).GetText() + " ya esta definida");
            
                     }

                 }

             }
        
      
        }
        catch(Exception e){

            msgError = msgError + e.Message + "\n";
        }
      

        return null;

    }

    public override object VisitClassDeclAST([NotNull] Parser1.ClassDeclASTContext context)
    {

        context.CLASE();
        tablaActual = 1;
        string idClass = context.ID().GetText();
        if (tableClases.buscarCl(idClass) != null)
        {

            msgError = msgError +"Linea: " + context.ID().Symbol.Line + "-> "+"La clase " + context.ID().GetText() + " ya esta definida" + "\n";

        }
        else{
            tableClases.insertarClase(idClass);
            if (context.varDecl() != null)
            {
                for (int i = 0; i <= context.varDecl().Length - 1; i++)
                {
                    Visit(context.varDecl(i));
                }
                tablaActual = 0;
            }
        }

        return null;

    }




    //Aun no terminado

    public override object VisitMethodDeclAST([NotNull] Parser1.MethodDeclASTContext context)
    {
        nivelActual++; // Aumenta el nivel para saber q son locales
        string[] arrayTipos = { }; // Guarda los tipos de parametros para insertarlo en la tabla de métodos
        string tipoMethod = ""; // Guarda el tipo del método
        string idMethod = ""; //Variable  que guarda le nombre del método actual
        idMethod = context.ID().GetText();

        // VALIDAR SI YA EXISTE
        if(tableMethods.buscarPNombre(idMethod)!= null)
        {
            Console.WriteLine("Ya existe un método con el id " + idMethod); // si el método ya existe retorna null
            msgError = msgError + "Linea: " + context.ID().Symbol.Line + "-> " + "Ya existe un método con el id " + idMethod + "\n";

            return null;
        }

        try
        {
            // BUSCAR EL TIPO DE FUNCIÓN

            if (context.VOID() != null) //En caso de que sea void devuelve 9 
            {
                tipoMethod = "void";
            }
            else if (context.type() != null) // Aqui se busca el tipo de el metodo en caso de que no sea void
            {
                tipoMethod = (string)Visit(context.type());
            }

         
            //  PARAMETROS

            context.PIZQ();

            if (context.formPars() != null)// en caso de que tenga parametros se recorre la lista y se guarda en un arreglo
            {
                List<string> listTip = (List<string>)Visit(context.formPars());

                arrayTipos = listTip.ToArray();

            }
            context.PDER();

            //INSETAR EL MÉTODO EN LA TABLA
            tableMethods.insertar(idMethod, tipoMethod, arrayTipos);// Inserta el método en la tablaMetodos
            nombreMetodo_Llamada = idMethod;
            


            // DECLARACIÓN DE VARIABLES 

            if (context.varDecl() != null) // si tiene declaración de variables se guardan en la tabla general
            {
                for (int i = 0; i <= context.varDecl().Length - 1; i++)
                {
                    Visit(context.varDecl(i));
                }
                tablaActual = 0;
            }

            // VISITAR EL BLOQUE 
        

            Visit(context.block());
            if (tipoMethod != "void" && retornoFun == false) {
                    throw new Exception("Linea: " + context.ID().Symbol.Line + "-> " + "Error esta función debe tener retorno");
            }
            
            if (tipoMethod == "void" && retornoFun == true)
            {
                throw new Exception("Linea: " + context.ID().Symbol.Line + "-> " + "Error función no debe tener retorno");
            }
            

            retornoFun = false;
            
            
        }
        catch(Exception e)
        {
            msgError = msgError + e.Message + "\n";
            retornoFun = false;
        }
        // CERRAR NIVEL
        table.cerrarNivel(); // Elimina las variables locales de la tabla
        nivelActual--;
        return null;
    }

    //************************************************  PARAMETROS DE UNA FUNCION *************************************************************

    public override object VisitFormParsAST([NotNull] Parser1.FormParsASTContext context)
    {
        try
        {
            string tipopAct = (string)Visit(context.type(0));
            string idAct = context.ID(0).GetText();

            List<string> list = new List<string>();
            list.Add(tipopAct);


            table.insertar(idAct, nivelActual, tipopAct, "param");

            if (context.type(1) != null)
            {
                for (int i = 1; i <= context.type().Length - 1; i++)
                {
                    tipopAct = (string)Visit(context.type(i));
                    idAct = context.ID(i).GetText();


                    table.insertar(idAct, nivelActual, tipopAct, "param");
                    list.Add(tipopAct);
                }

            }

            return list;
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }

    }

    //************************************************  TIPOS *************************************************************

    public override object VisitTypeAST([NotNull] Parser1.TypeASTContext context)
    {
        string var = null;
        if (context.ID().GetText() == "int")
        {
            if (context.PCUADRADO_IZQ() != null)
            {
                var = "int[]";
            }
            else
            {
                var = "int";
            }
        }
        else if (context.ID().GetText() == "char")
        {
            if (context.PCUADRADO_IZQ() != null)
            {
                var = "char[]";
            }
            else
            {
                var = "char";
            }
        }
        else if (context.ID().GetText() == "float")
        {
            if (context.PCUADRADO_IZQ() != null)
            {
                throw new Exception("Linea: " + context.ID().Symbol.Line + "-> " + "No existen arreglos tipo float"); // si el tipo es indefinido
            }
            else
            {
                var = "float";
            }
        }
        else if (context.ID().GetText() == "boolean")
        {
            if (context.PCUADRADO_IZQ() != null)
            {
                throw new Exception("Linea: " + context.ID().Symbol.Line + "-> " + "No existen arreglos tipo boolean"); // si el tipo es indefinido var = "boolean[]";
            }
            else
            {
                var = "boolean";
            }
        }
        else {
            if (tableClases.buscarCl(context.ID().GetText()) != null)
            {
                if (context.PCUADRADO_IZQ() == null)
                {
                    var = (context.ID().GetText()); // si es tipo clase
                }
                else
                {
                    throw new Exception("Linea: " + context.ID().Symbol.Line + "-> "+"No existe arreglos tipo clase"); // si el tipo es indefinido
                }
               
            }
            else {
                throw new Exception("Linea: " + context.ID().Symbol.Line + "-> " + "No existe este tipo de dato"); // si el tipo es indefinido
            }
        }

        return var;
    }

    //************************************************  STATEMENT *************************************************************

    public override object VisitDesignatorStatAST([NotNull] Parser1.DesignatorStatASTContext context)
    {
        try
        {
            string tipoID = (string)Visit(context.designator());
            if (tipoID == "null")
            {
                throw new Exception("Linea: " + context.ASIGN().Symbol.Line + "-> " + "Error no se puede asignar algo a la palabra reservada null");
           }

            if (context.ASIGN() != null)
            {
                string tipoExpre = (string)Visit(context.expr());

                if ((tipoExpre == "int[]" || tipoExpre == "char[]") && esNew == false)
                {
                    throw new Exception("Linea: " + context.ASIGN().Symbol.Line + "-> " + "Error no se pueden asignar arreglos directamente");
                }

                if (banderaClaseVar == "const")
                {
                    throw new Exception("Linea: " + context.ASIGN().Symbol.Line + "-> " + "No se le pueden asignar valores a una constante");
                }
                
                 if (tipoExpre != tipoID && tipoID != "null")
                 {
                     throw new Exception("Linea: " + context.ASIGN().Symbol.Line + "-> " + "Tipos Incompatibles ( " + tipoID + "," + tipoExpre + ")");
                 }

                 esNew = false;
            }
               //*************************************************
            else if (context.PIZQ() != null && tipoID == "metodo")
            {
                TablaMetodos.ElementoMet met = tableMethods.buscarPNombre(nombreMetodo_Llamada);
                string tipoMethod = met.tipo;//tipo del metodo
                string[] arrTiposParam = null;
                if (context.actPars() != null)
                {
                     arrTiposParam = (string[])Visit(context.actPars());// arreglo de tipos de parametros
                }
                else {
                    arrTiposParam = new string[0];
                    
                }
                string idMethod = nombreMetodo_Llamada;// nombre del método
                if (met.numPara != arrTiposParam.Length)
                {
                    throw new Exception("Linea: " + context.PIZQ().Symbol.Line + "-> " + "Error se esperanban " + met.numPara + " argumentos y se tienen" + arrTiposParam.Length);
                }


                else
                {
                    for (int i = 0; i < arrTiposParam.Length; i++)
                    {
                        if (met.tiposPara[i] == "array[]")
                        {
                            if (arrTiposParam[i] != "char[]" && arrTiposParam[i] != "int[]" )
                            {
                                throw new Exception("Linea: " + context.PIZQ().Symbol.Line + "-> " + "Tipos de parametros no son los  correctos");
                            }

                        }
                        else
                        {
                            if (arrTiposParam[i] != met.tiposPara[i])
                            {
                                throw new Exception("Linea: " + context.PIZQ().Symbol.Line + "-> " + "Tipos de parametros no son los  correctos");
                            }
                        }


                    }
                    return tipoMethod;
                }
            }


            //*******************************************************
            else if (context.INCRE() != null || context.INCRE() != null)
            {
                if (tipoID != "int")
                {
                    throw new Exception("Linea: " + context.INCRE().Symbol.Line + "-> " + "Solo se puede incrementar o decrementar variables de tipo int");
                }
            }

           
        }
        catch(Exception e){
            Console.WriteLine( e.Message);
            msgError = msgError + e.Message + "\n";
        }
        return null;
    }

    public override object VisitIfStatAST([NotNull] Parser1.IfStatASTContext context)
    {
        context.CONDICION_IF();
        context.PIZQ();
        //Entrando el retorno es falso
        tRetorno = false;
     
            Visit(context.condition());

         try
        {
            context.PDER();

            if (context.CONDICION_ELSE() != null)
            {
                //Si existe else significa que puede que exista retorno en los dos
                tRetorno = true;
            }
            Visit(context.statement(0));

             
            if (context.CONDICION_ELSE() != null)
            {
                // En el else se erifica que el if tuviera return
                if (retornoFun == false)
                {
                //En caso de no ser asi ya no existe posibilidad deque el retorno sea correcto
                    tRetorno = false;
                }
                else {
                // De lo contrarios la variable retorno se vuelve al estado original
                    retornoFun = false;
                // Y la posibilidad de que el retorno se presente vuelve a ser positivo
                    tRetorno = true;
                }
                Visit(context.statement(1));
                tRetorno = true;
            }
           
         } 
        catch(Exception e){
            tRetorno = true;
            msgError = msgError + e.Message + "\n";
        }
        return null;
    }

    public override object VisitForStatAST([NotNull] Parser1.ForStatASTContext context)
    {
        tRetorno = false;
       context.CICLO_FOR();
            context.PIZQ();
            Visit(context.expr());
            context.PyCOMA();
            if (context.condition() != null)
            {
                Visit(context.condition());
            }

        try
        {

            context.PyCOMA();
            if (context.statement() != null)
            {
                Visit(context.statement(0));
                context.PDER();
                Visit(context.statement(1));
                tRetorno = true;
            }
            else
            {
                context.PDER();
                Visit(context.statement(0));
                tRetorno = true;
            }
           
        }
        catch(Exception e)
        {
            tRetorno = true;
            Console.WriteLine(e.Message);
            msgError = msgError + e.Message + "\n";
        }
        return null;
    }
    public override object VisitWhileStatAST([NotNull] Parser1.WhileStatASTContext context)
    {
        tRetorno = false;
        
            context.CICLO_WHILE();
            context.PIZQ();
            Visit(context.condition());
            context.PDER();
        try
        {
            Visit(context.statement());
            tRetorno = true;
            
        }
         catch (Exception e)
         {
             tRetorno = true;
             Console.WriteLine(e.Message);
             msgError = msgError + e.Message + "\n";
         }
         return null;
    }
    //FALTA
    public override object VisitForeachStatAST([NotNull] Parser1.ForeachStatASTContext context)
    {
        tRetorno = false;
        try
        {

            context.CICLO_FOREACH();
            context.PIZQ();
            string tipo = (string)Visit(context.type());
            string expr = (string)Visit(context.expr());

            if (tipo != "int" && tipo != "char")
            {
                throw new Exception("Linea: " + context.PIZQ().Symbol.Line + "-> " + "Error solo se admiten variables tipo int y char");
            }

            if ((tipo == "int" && expr != "int[]") || (tipo == "char" && expr != "char[]") )
            {
                throw new Exception("Linea: " + context.PIZQ().Symbol.Line + "-> " + "Error la variable debe ser del mismo tipo del arreglo");
            }

            tRetorno = true;
        }
        catch (Exception e)
        {
             tRetorno = true;
            Console.WriteLine(e.Message);
            msgError = msgError + e.Message + "\n";

        }
         return null;
    }

    public override object VisitReturnStatAST([NotNull] Parser1.ReturnStatASTContext context)
    {
        if (tRetorno == true) {
            retornoFun = true;
        }
        try
        {
            string tipoR = (string)Visit(context.expr());
            string tipoMe = tableMethods.ultimoTipo();
            if (tipoR != tipoMe)
            {
                throw new Exception("Linea: " + context.RETURN().Symbol.Line + "-> " + "Tipos Incompatibles ( Retorno:" + tipoR + ", Función:" + tipoMe + ")");
            }
        }

        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            msgError = msgError + e.Message + "\n";
        }
        return null;
    }
    public override object VisitReadStatAST([NotNull] Parser1.ReadStatASTContext context)
    {
        try
        {
            // READ PIZQ designator PDER PyCOMA	
            context.READ();
            context.PIZQ();
            Visit(context.designator());
            context.PDER();
        }

        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            msgError = msgError + e.Message + "\n";
        }
        return null;
    }

    public override object VisitWriteStatAST([NotNull] Parser1.WriteStatASTContext context)
    {
        try
        {
            //WRITE PIZQ expr (COMA NUMBER)? PDER PyCOMA	
            context.WRITE();
            context.PIZQ();
            Visit(context.expr());
            //creo q no es necesario
            if (context.NUMBER() != null)
            {
                context.NUMBER();
            }
            context.PDER();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            msgError = msgError + e.Message + "\n";
        }
        return null;
    }

    //************************************************  BLOCK *************************************************************

    public override object VisitBlockStatAST([NotNull] Parser1.BlockStatASTContext context)
    {
        Visit(context.block());
        
        return null;
    }


    public override object VisitBlockAST([NotNull] Parser1.BlockASTContext context)
    {
        for (int i = 0; i <= context.statement().Length - 1; i++)
        {
            Visit(context.statement(i));
        }
        return null;

    }
    //*****************************************  PARAMETOS(LLAMADA A FUNCION) **********************************************************

    public override object VisitActParsAST([NotNull] Parser1.ActParsASTContext context)
    {
        //: expr (COMA expr)*	
        List<string> tipos = new List<string>();
        try
        {
            string tipo1Ex = (string)Visit(context.expr(0));
            tipos.Add(tipo1Ex);
            for (int i = 1; i <= context.expr().Length - 1; i++)
            {
                tipos.Add((string)Visit(context.expr(i)));
            }

            return tipos.ToArray();
        }
        catch(Exception e){
            throw new Exception(e.Message);
        }
    }

    //xq boolean
    //************************************************  CONDITION *************************************************************

    public override object VisitConditionAST([NotNull] Parser1.ConditionASTContext context)
    {
        int i = 1;
        try
        {
            Visit(context.condTerm(0));

            if (context.condTerm(1) != null)
            {
                for (i = 1; i <= context.condTerm().Length - 1; i++)
                {
                    Visit(context.condTerm(i));
                }
            }
            return null;
        }
        catch (Exception e)
        {
            msgError = msgError  + e.Message + "\n";
 
        }
        return null;
    }

    //xq boolean
    public override object VisitCondTermAST([NotNull] Parser1.CondTermASTContext context)
    {
        try
        {
            Visit(context.condFact(0));

            if (context.condFact(1) != null)
            {
                for (int i = 1; i <= context.condFact().Length - 1; i++)
                {
                    Visit(context.condFact(i));
                }
            }
            return null;
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }

        
    }



    public override object VisitCondFactAST([NotNull] Parser1.CondFactASTContext context)
    {

        if ((context.relop().GetText() == "==") || (context.relop().GetText() == "!="))
        {
            string tipo1 = (string)Visit(context.expr(0));
            string tipo2 = (string)Visit(context.expr(1));
            if (tipo1 != tipo2)
            {
                //pendiente
                throw new Exception("Linea: " + lineaActual + "-> Los dos tipos a comparar no son del mismo tipo");
            }

        }
        else
        {
            string tipo1 = (string)Visit(context.expr(0));
            string tipo2 = (string)Visit(context.expr(1));
            if (tipo1 == tipo2)
            {
                if (tipo1 != "int" && tipo1 != "float")
                {
                    throw new Exception("Linea: " + lineaActual + "-> Solo se pueden hacer esas comparaciones entre tipos int y float");
                }
                /*else {
                    return true;// se hace para saber q todo está bien
                }
                 * */

            }
            else
            {
                throw new Exception( "Linea: " + lineaActual + "-> Los dos tipos a comparar no son del mismo tipo");
                //Lanzar exe
                // return false;
            }


        }
        return null;

    }



    //************************************************  EXPRESIONES *************************************************************

    public override object VisitExprAST([NotNull] Parser1.ExprASTContext context)
    {
        try
        {
            string tipo1, tipo2 = "";

            tipo1 = (string)Visit(context.term(0));

            for (int i = 1; i <= context.term().Length - 1; i++)
            {

                tipo2 = (string)Visit(context.term(i));
                if (tipo1 != tipo2)
                {
                    //pendiente
                    throw new Exception("Linea: " + lineaActual + " -> Tipos Incompatibles (" + tipo1 + ", " + tipo2 + ")");

                }
                if ((tipo1 != "int") && (tipo2 != "float"))
                {
                    throw new Exception("Linea: " + lineaActual + " -> Error: Esta operación solo se puede hacer con float e int");
                    
                }
                tipo1 = tipo2;

            }
             return tipo1;
        }
       
    
    catch(Exception e){
        throw new Exception(e.Message);
    }
     
    }
    //************************************************  TERMINALES *************************************************************

    public override Object VisitTermAST([NotNull] Parser1.TermASTContext context)
    {
        string tipo1, tipo2 = "";

        try
        {
            tipo1 = (string)Visit(context.factor(0));

            for (int i = 1; i <= context.factor().Length - 1; i++)
            {

                tipo2 = (string)Visit(context.factor(i));
                if (tipo1 != tipo2)
                {
                    throw new Exception("Linea: " + lineaActual + " -> Tipos Incompatibles (" + tipo1 + ", " + tipo2 + ")");
                
                }
                if ((tipo1 != "int") && (tipo2 != "float"))
                {
                    throw new Exception("Linea: " + lineaActual + " -> Error esta operación solo se puede hacer con float e int");
                
                }
                tipo1 = tipo2;

            }
            return tipo1;
        }

        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
  
    }

    //************************************************  FACTOR *************************************************************

    public override object VisitDesignatorFactorAST([NotNull] Parser1.DesignatorFactorASTContext context)
    {
        try
        {
            string tipoid = (string)Visit(context.designator());


            if (context.PIZQ() != null && tipoid == "metodo")
            {
                TablaMetodos.ElementoMet met = tableMethods.buscarPNombre(nombreMetodo_Llamada);
                string tipoMethod = met.tipo;//tipo del metodo
                string[] arrTiposParam = null;
                if (context.actPars() != null)
                {
                    arrTiposParam = (string[])Visit(context.actPars());// arreglo de tipos de parametros
                }
                else
                {
                    arrTiposParam = new string[0];

                }
                string idMethod = nombreMetodo_Llamada;// nombre del método
                if (met.numPara != arrTiposParam.Length)
                {
                    throw new Exception("Linea: " + context.PIZQ().Symbol.Line + "-> " + "Error se esperanban " + met.numPara + " argumentos y se tienen" + arrTiposParam.Length);
                }


                else
                {
                    for (int i = 0; i < arrTiposParam.Length; i++)
                    {
                        if (met.tiposPara[i] == "array[]")
                        {
                            if (arrTiposParam[i] != "char[]" && arrTiposParam[i] != "int[]"  )
                            {
                                throw new Exception("Linea: " + context.PIZQ().Symbol.Line + "-> " + "Tipo de parametros no son los  correctos");
                            }

                        }
                        else
                        {
                            if (arrTiposParam[i] != met.tiposPara[i])
                            {
                                throw new Exception("Linea: " + context.PIZQ().Symbol.Line + "-> " + "Tipo de parametros no son los  correctos");
                            }
                        }


                    }
                    return tipoMethod;
                }
            }

            else
            {
                return tipoid;
            }
        }
        catch(Exception e) {
            throw new Exception(e.Message);
        }
    }

    //HECHOS
    public override object VisitNumberFactorAST([NotNull] Parser1.NumberFactorASTContext context)
    {
        lineaActual = context.NUMBER().Symbol.Line;
        return "int";
    }
    public override object VisitCharconstFactorAST([NotNull] Parser1.CharconstFactorASTContext context)
    {
        lineaActual = context.CharConst().Symbol.Line;
        return "char";
    }

    public override object VisitFloatFactorAST([NotNull] Parser1.FloatFactorASTContext context)
    {
        lineaActual = context.FLOAT().Symbol.Line;
        return "float";
    }



    public override object VisitTruefalseFactorAST([NotNull] Parser1.TruefalseFactorASTContext context)
    {
        if (context.TRUE() != null)
        {
            lineaActual = context.TRUE().Symbol.Line;
        }
        else if (context.FALSE() != null)
        {
            lineaActual = context.FALSE().Symbol.Line;
        }
        return "boolean";
    }
    public override object VisitNewFactorAST([NotNull] Parser1.NewFactorASTContext context)
    {
        esNew = true;
        lineaActual = context.ID().Symbol.Line;
        string var = "";
        if (context.ID().GetText() == "int")
        {
            if (context.PCUADRADO_IZQ() != null)
            {
                if ((string)Visit(context.expr()) == "int")
                {
                    var = "int[]";
                }
                else
                {
                    throw new Exception("Linea: " + context.ID().Symbol.Line + "-> " + "Error de contenido de arreglo, debe ser tipo int");

                }
            }
            else
            {
                var = "int";
            }
        }
        else if (context.ID().GetText() == "char")
        {
            if (context.PCUADRADO_IZQ() != null)
            {
                if ((string)Visit(context.expr()) == "int")
                {
                    var = "char[]";
                }
                else
                {
                    throw new Exception("Linea: " + context.ID().Symbol.Line + "-> " + "Error de contenido de arreglo, debe ser tipo int");
                }
            }
            else
            {
                var = "char";
            }
        }
        else if (context.ID().GetText() == "float")
        {
            if (context.PCUADRADO_IZQ() != null)
            {
             
                    throw new Exception("Linea: " + context.ID().Symbol.Line + "-> " + "Error no existen arreglos tipo float");
  
      
            }
            else
            {
                var = "float";//preguntar si se puede instanciar int,float,char
            }
        }
        else if (context.ID().GetText() == "boolean")
        {
            if (context.PCUADRADO_IZQ() != null)
            {
                throw new Exception("Linea: " + context.ID().Symbol.Line + "-> " + "Error no existen arreglos tipo boolean");
            }
  
        }
        else {
            if (tableClases.buscarCl(context.ID().GetText()) != null)
            {
                if (context.PCUADRADO_IZQ() != null)
                {
                    throw new Exception("Linea: " + context.PCUADRADO_IZQ().Symbol.Line + "Error no existen arreglos tipo clase");

                }
                else
                    var = tableClases.buscarCl(context.ID().GetText()).nombre;
            }
            else {
                throw new Exception("Linea: " + context.ID().Symbol.Line + "Error no existe este tipo de dato");

            }
        }

        return var;

    }
    public override object VisitExprFactorAST([NotNull] Parser1.ExprFactorASTContext context)
    {

        string tipoExp = (string)Visit(context.expr());
        return tipoExp;
    }

    //********************************************  DESIGNATOR(VAR,CLASES,METODOS) *****************************************************

    public override object VisitDesignatorAST([NotNull] Parser1.DesignatorASTContext context)
    {
        string tipoARet = context.ID(0).GetText();
        // si hay mas de dos ID
        if (context.ID(1) != null && table.buscar(context.ID(0).GetText()) != null)
        {   
            string tipo = table.buscar(context.ID(0).GetText()).tipo;
             // Si el primer elemento es de tipo clase
             if (tipo != "int" && tipo != "char" && tipo != "float"
                && tipo != "int[]" && tipo != "char[]" )
            {
                tipoARet = tipo;
                string tipoAnt = tipoARet;
                //Recorrido de los ID
                for (int i = 1; i < context.ID().Length; i++)
                {
                    if (i > 1) {

                        tipoAnt = tipoARet;
                    }

                    if (tipoAnt != "int" && tipoAnt != "char" && tipoAnt != "float"
                        && tipoAnt != "int[]" && tipoAnt != "char[]"  )// significa q es tipo clase
                    {

                        if (tableClases.buscarPNombre(context.ID(i).GetText(), tipoAnt) == null) // si el atributo pertenece a la clase
                        {
                            throw new Exception("Linea: " + context.ID(i).Symbol.Line + "-> No existe el atributo " + context.ID(i).GetText() + " en la clase " + tipoAnt);
                            
                        }
                        else {
                            //si la variable tiene expresion
                            if (context.expr(0) != null)
                            {
                                if (context.expr(1) != null) {
                                    throw new Exception("Linea: " + context.ID(i).Symbol.Line + "-> Solo existen arreglos simples");
                                }

                                string  elem = tableClases.buscarPNombre(context.ID(i).GetText(), tipoAnt).tipo; ;
                                //si es de tipo array int o char
                                if (elem == "int[]" || elem == "char[]")
                                {
                                    string tipoEx = (string)Visit(context.expr(0));
                                    //si la expre es diferente a int
                                    if (tipoEx != "int")
                                    {
                                        throw new Exception("Linea: " + context.ID(i).Symbol.Line + "-> La expresión debe que ser de tipo int");
                                    }
                                    //si todo esta bien
                                    else
                                    {
                                        if (elem == "int[]")
                                        {
                                            return "int";
                                        }
                                        else{
                                            return "char";
                                        }
                                       
                                    }

                                }
                                //si no es de tipo array int o char
                                else
                                {
                                    throw new Exception("Linea: " + context.ID(i).Symbol.Line + "-> La variable " + context.ID(i).GetText() + " no es de tipo arreglo");
                                }


                            }

                            else {
                                tipoARet = tableClases.buscarPNombre(context.ID(i).GetText(), tipoAnt).tipo; ;
                            
                            }
                           
                        }
                    }
                    else
                    {
                        throw new Exception("Linea: " + context.ID(i).Symbol.Line + "-> La variable " + context.ID(i-1).GetText()+ " no es de tipo clase");
                        
                    }
                }
                return tipoARet;
            }
            else {
                throw new Exception("Linea: " + context.ID(0).Symbol.Line + "-> Variable " + context.ID(0).GetText()+ " no es tipo clase");
           
            }
              
            
        }
        else if (context.expr(0) != null)

        {
            if (context.expr(1) != null)
            {
                throw new Exception("Linea: " + context.ID(0).Symbol.Line + "-> Solo existen arreglos simples");
            }
             TablaSimbolos.ElementoG elem = table.buscar(context.ID(0).ToString());
            //si no existe la variable
            if(elem == null){
                throw new Exception("Linea: " + context.ID(0).Symbol.Line + "-> No existe un arreglo con ese ID");
            }
            //si es de tipo array int o char
            if (elem.tipo == "int[]" || elem.tipo == "char[]")
            {
                string tipoEx = (string)Visit(context.expr(0));
                //si la expre es diferente a int
                if (tipoEx != "int")
                {
                    throw new Exception("Linea: " + context.ID(0).Symbol.Line + "-> La expresión tiene que ser de tipo int");
                }
                //si todo esta bien
                else
                {
                    if (elem.tipo == "int[]")
                    {
                        return "int";
                    }
                    else
                    {
                        return "char";
                    }

                }

            }
            //si no es de tipo array int o char
            else {
                throw new Exception("Linea: " + lineaActual + "-> La variable no es de tipo arreglo");
            }
           
           
        }
        else
        {
   
            TablaSimbolos.ElementoG elem = table.buscar(context.ID(0).ToString());

            TablaMetodos.ElementoMet elem2 = tableMethods.buscarPNombre(context.ID(0).ToString());
            if (elem != null)
            {
                banderaClaseVar = table.buscar(context.ID(0).GetText()).claseEl;
                return elem.tipo;
            }
            else if (elem2 != null)
            {
                nombreMetodo_Llamada = context.ID(0).ToString();
                return "metodo";
            }
            else
            {   
                throw new Exception("Linea: " + context.ID(0).Symbol.Line + "-> Variable " + context.ID(0).ToString() + " no definida");
            }
        }
    }



    //************************************************  COMPARACIONES *************************************************************

    //#comparacionRelopAST
    public override object VisitComparacionRelopAST([NotNull] Parser1.ComparacionRelopASTContext context)
    {

        return "==";
    }
    //	#diferenteRelopAST
    public override object VisitDiferenteRelopAST([NotNull] Parser1.DiferenteRelopASTContext context)
    {
        return "!=";
    }
    //	#mayorRelopAST
    public override object VisitMayorRelopAST([NotNull] Parser1.MayorRelopASTContext context)
    {
        return ">";
    }
    //	#mayorigualRelopAST
    public override object VisitMayorigualRelopAST([NotNull] Parser1.MayorigualRelopASTContext context)
    {
        return ">=";
    }
    //	#menorRelopAST

    public override object VisitMenorRelopAST([NotNull] Parser1.MenorRelopASTContext context)
    {
        return "<";
    }
    //	#menorigualRelopAST

    public override object VisitMenorigualRelopAST([NotNull] Parser1.MenorigualRelopASTContext context)
    {
        return "<=";
    }

   public override object VisitMulMulopAST([NotNull] Parser1.MulMulopASTContext context){
       lineaActual = context.MUL().Symbol.Line;
       return "*";
   }
   public override object VisitDivMulopAST([NotNull] Parser1.DivMulopASTContext context){
       lineaActual = context.DIV().Symbol.Line;
       return "/";
   }
   public override object VisitDivmodMulopAST([NotNull] Parser1.DivmodMulopASTContext context){
       lineaActual = context.DIVMOD().Symbol.Line;
       return "%";
   }
   public override object VisitSumaAddopAST([NotNull] Parser1.SumaAddopASTContext context){
       lineaActual = context.SUMA().Symbol.Line;
       return "+";
   }

   public override object VisitRestaAddopAST([NotNull] Parser1.RestaAddopASTContext context) {
       lineaActual = context.RESTA().Symbol.Line;
       return "-";
   }


//: MUL																							#mulMulopAST
//| DIV																							#divMulopAST
//| DIVMOD																						#divmodMulopAST

//: SUMA																							#sumaAddopAST
//| RESTA																							#restaAddopAST




    /**********************************************************************************************************************************/

}
