using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using AnalizadorSintactico;
using System.Windows.Forms;

class PrettyPrint : Parser1BaseVisitor<Object>
{

    //private var for tree
    private TreeView treeview;

    public PrettyPrint(TreeView tree)
    {
        this.treeview = tree;
    }

    public override object VisitClassDeclAST([NotNull] Parser1.ClassDeclASTContext context)
    {
        TreeNode clase = new TreeNode(context.CLASE().ToString());
        TreeNode ID = new TreeNode(context.ID().ToString());
        TreeNode CD = new TreeNode(context.COR_DER().ToString());
        TreeNode CI = new TreeNode(context.COR_IZQ().ToString());
        int largo = 4 + context.varDecl().Count();
        TreeNode[] arreglo = new TreeNode[largo];
        arreglo[0] = clase;
        arreglo[1] = ID;
        arreglo[2] = CD;
        int cont = 3;
        for (int i = 0; i <= context.varDecl().Count() - 1; i++)
        {
            TreeNode vardecl = (TreeNode)Visit(context.varDecl(i));
            arreglo[cont] = vardecl;
            cont++;
        }
        arreglo[cont] = CI;
        TreeNode final = new TreeNode("ClassDecl", arreglo);
        return final;
    }


    public override object VisitSumaAddopAST([NotNull] Parser1.SumaAddopASTContext context)
    {
        TreeNode suma = new TreeNode(context.SUMA().ToString());
        TreeNode[] arreglo = new TreeNode[] { suma };
        TreeNode final = new TreeNode("Addop-Suma", arreglo);
        return final;
    }


    public override object VisitRestaAddopAST([NotNull] Parser1.RestaAddopASTContext context)
    {
        TreeNode resta = new TreeNode(context.RESTA().ToString());
        TreeNode[] arreglo = new TreeNode[] { resta };
        TreeNode final = new TreeNode("Addop-Resta", arreglo);
        return final;
    }


    public override object VisitCondTermAST([NotNull] Parser1.CondTermASTContext context)
    {
        TreeNode condfact1 = (TreeNode)Visit(context.condFact(0));
        int largo = context.Y().Count() + context.condFact().Count();
        TreeNode[] arreglo = new TreeNode[largo];
        arreglo[0] = condfact1;
        int count = 1; //contador del arreglo
        int iAnd = 0; //contador para AND
        for (int i = 1; i <= context.condFact().Count() - 1; i++)
        {
            TreeNode and = new TreeNode(context.Y(iAnd).ToString());
            TreeNode condfact2 = (TreeNode)Visit(context.condFact(i));
            arreglo[count] = and;
            arreglo[count + 1] = condfact2;
            count += 2;
            iAnd++;
        }
        TreeNode final = new TreeNode("CondTerm", arreglo);
        return final;
    }


    public override object VisitConstDeclAST([NotNull] Parser1.ConstDeclASTContext context)
    {
        TreeNode constante = new TreeNode(context.CONSTANTE().ToString());
        TreeNode type = (TreeNode)Visit(context.type());
        TreeNode ID = new TreeNode(context.ID().ToString());
        TreeNode asign = new TreeNode(context.ASIGN().ToString());
        TreeNode[] arreglo = new TreeNode[6];
        arreglo[0] = constante;
        arreglo[1] = type;
        arreglo[2] = ID;
        arreglo[3] = asign;

        int cont = 4;

        if (context.NUMBER() != null)
        {
            TreeNode number = new TreeNode(context.NUMBER().ToString());
            arreglo[cont] = number;
            cont++;
        }
        else if (context.CharConst() != null)
        {
            TreeNode charconst = new TreeNode(context.CharConst().ToString());
            arreglo[cont] = charconst;
            cont++;
        }
        else if (context.FLOAT() != null)
        {
            TreeNode floatCosn = new TreeNode(context.FLOAT().ToString());
            arreglo[cont] = floatCosn;
            cont++;
        }
        TreeNode pycoma = new TreeNode(context.PyCOMA().ToString());
        arreglo[cont] = pycoma;
        TreeNode final = new TreeNode("ConstDecl", arreglo);
        return final;
    }


    public override object VisitProgramAST([NotNull] Parser1.ProgramASTContext context)
    {
        TreeNode Clase = new TreeNode(context.CLASE().ToString());
        TreeNode ID = new TreeNode(context.ID().ToString());
        TreeNode CD = new TreeNode(context.COR_DER().ToString());
        TreeNode CI = new TreeNode(context.COR_IZQ().ToString());

        int largo = 4;
        //estimar el tamaño del arreglo a usar conociendo si vienen datos en dichas variables o no
        if (context.constDecl() != null) { largo += context.constDecl().Count(); }
        if (context.classDecl() != null) { largo += context.classDecl().Count(); }
        if (context.varDecl() != null) { largo += context.varDecl().Count(); }
        largo += context.methodDecl().Count();


        TreeNode[] arreglo = new TreeNode[largo];
        arreglo[0] = Clase;
        arreglo[1] = ID;
        int cont = 2;//estado actual de posición del arreglo

        //recorrer las variables en un ciclo porque se ejecutan una o más veces
        if (context.constDecl() != null)
        {
            for (int i = 0; i < context.constDecl().Count(); i++)
            {
                arreglo[cont] = (TreeNode)Visit(context.constDecl(i));
                cont++;
            }
        }
        if (context.varDecl() != null)
        {
            for (int i = 0; i < context.varDecl().Count(); i++)
            {
                arreglo[cont] = (TreeNode)Visit(context.varDecl(i));
                cont++;
            }
        }
        if (context.classDecl() != null)
        {
            for (int i = 0; i < context.classDecl().Count(); i++)
            {
                arreglo[cont] = (TreeNode)Visit(context.classDecl(i));
                cont++;
            }
        }

        arreglo[cont] = CD; //llave derecha
        cont++;
        for (int i = 0; i < context.methodDecl().Count(); i++)
        {
            arreglo[cont] = (TreeNode)Visit(context.methodDecl(i));
            cont++;
        }
        arreglo[cont] = CI;
        TreeNode final = new TreeNode("Program", arreglo);
        treeview.Nodes.Add(final);
        return null; //en caso de ERROR cambiar a null
    }


    public override object VisitMethodDeclAST([NotNull] Parser1.MethodDeclASTContext context)
    {
        TreeNode[] arreglo;
        if (context.formPars() != null)
        {
            if (context.type() != null)
            {
                TreeNode type = (TreeNode)Visit(context.type());
                TreeNode ID = new TreeNode(context.ID().ToString());
                TreeNode PIZQ = new TreeNode(context.PIZQ().ToString());
                TreeNode formpars = (TreeNode)Visit(context.formPars());
                TreeNode PDER = new TreeNode(context.PDER().ToString());
                TreeNode block = (TreeNode)Visit(context.block());
                int largo = 6 + context.varDecl().Count();
                arreglo = new TreeNode[largo];
                arreglo[0] = type;
                arreglo[1] = ID;
                arreglo[2] = PIZQ;
                arreglo[3] = formpars;
                arreglo[4] = PDER;
                int cont = 5; //contador arreglo
                for (int i = 0; i <= context.varDecl().Count() - 1; i++)
                {
                    TreeNode varDecl = (TreeNode)Visit(context.varDecl(i));
                    arreglo[cont] = varDecl;
                    cont++;
                }
                arreglo[cont] = block;
            }
            else
            {
                TreeNode voids = new TreeNode(context.VOID().ToString());
                TreeNode ID = new TreeNode(context.ID().ToString());
                TreeNode PIZQ = new TreeNode(context.PIZQ().ToString());
                TreeNode formpars = (TreeNode)Visit(context.formPars());
                TreeNode PDER = new TreeNode(context.PDER().ToString());
                TreeNode block = (TreeNode)Visit(context.block());
                int largo = 6 + context.varDecl().Count();
                arreglo = new TreeNode[largo];
                arreglo[0] = voids;
                arreglo[1] = ID;
                arreglo[2] = PIZQ;
                arreglo[3] = formpars;
                arreglo[4] = PDER;
                int cont = 5; //contador arreglo
                for (int i = 0; i <= context.varDecl().Count() - 1; i++)
                {
                    TreeNode varDecl = (TreeNode)Visit(context.varDecl(i));
                    arreglo[cont] = varDecl;
                    cont++;
                }
                arreglo[cont] = block;
            }
        }
        else
        {
            if (context.type() != null)
            {
                TreeNode type = (TreeNode)Visit(context.type());
                TreeNode ID = new TreeNode(context.ID().ToString());
                TreeNode PIZQ = new TreeNode(context.PIZQ().ToString());
                TreeNode PDER = new TreeNode(context.PDER().ToString());
                TreeNode block = (TreeNode)Visit(context.block());
                int largo = 5 + context.varDecl().Count();
                arreglo = new TreeNode[largo];
                arreglo[0] = type;
                arreglo[1] = ID;
                arreglo[2] = PIZQ;
                arreglo[3] = PDER;
                int cont = 4; //contador arreglo
                for (int i = 0; i <= context.varDecl().Count() - 1; i++)
                {
                    TreeNode varDecl = (TreeNode)Visit(context.varDecl(i));
                    arreglo[cont] = varDecl;
                    cont++;
                }
                arreglo[cont] = block;
            }
            else
            {
                TreeNode voids = new TreeNode(context.VOID().ToString());
                TreeNode ID = new TreeNode(context.ID().ToString());
                TreeNode PIZQ = new TreeNode(context.PIZQ().ToString());
                TreeNode PDER = new TreeNode(context.PDER().ToString());
                TreeNode block = (TreeNode)Visit(context.block());
                int largo = 5 + context.varDecl().Count();
                arreglo = new TreeNode[largo];
                arreglo[0] = voids;
                arreglo[1] = ID;
                arreglo[2] = PIZQ;
                arreglo[3] = PDER;
                int cont = 4; //contador arreglo
                for (int i = 0; i <= context.varDecl().Count() - 1; i++)
                {
                    TreeNode varDecl = (TreeNode)Visit(context.varDecl(i));
                    arreglo[cont] = varDecl;
                    cont++;
                }
                arreglo[cont] = block;
            }
        }
        TreeNode final = new TreeNode("MethodDecl", arreglo);
        return final;
    }


    public override object VisitTypeAST([NotNull] Parser1.TypeASTContext context)
    {
        TreeNode ID = new TreeNode(context.ID().ToString());
        TreeNode[] arreglo;
        if (context.PCUADRADO_IZQ() != null) //comprobar si viene llaves o no (0/1)
        {
            TreeNode PI = new TreeNode(context.PCUADRADO_IZQ().ToString());
            TreeNode PD = new TreeNode(context.PCUADRADO_DER().ToString());
            arreglo = new TreeNode[] { ID, PI, PD };
        }
        else
        {
            arreglo = new TreeNode[] { ID };
        }
        TreeNode final = new TreeNode("Type", arreglo);
        return final;
    }


    public override object VisitMayorigualRelopAST([NotNull] Parser1.MayorigualRelopASTContext context)
    {
        TreeNode mayorigual = new TreeNode(context.MAYORIGUAL().ToString());
        TreeNode[] arreglo = new TreeNode[] { mayorigual };
        TreeNode final = new TreeNode("Relop-MayorIgual", arreglo);
        return final;
    }


    public override object VisitMayorRelopAST([NotNull] Parser1.MayorRelopASTContext context)
    {
        TreeNode mayor = new TreeNode(context.MAYOR().ToString());
        TreeNode[] arreglo = new TreeNode[] { mayor };
        TreeNode final = new TreeNode("Relop-Mayor", arreglo);
        return final;
    }


    public override object VisitMenorRelopAST([NotNull] Parser1.MenorRelopASTContext context)
    {
        TreeNode menor = new TreeNode(context.MENOR().ToString());
        TreeNode[] arreglo = new TreeNode[] { menor };
        TreeNode final = new TreeNode("Relop-Menor", arreglo);
        return final;
    }


    public override object VisitDiferenteRelopAST([NotNull] Parser1.DiferenteRelopASTContext context)
    {
        TreeNode diferente = new TreeNode(context.DIFERENTE().ToString());
        TreeNode[] arreglo = new TreeNode[] { diferente };
        TreeNode final = new TreeNode("Relop-Diferente", arreglo);
        return final;
    }


    public override object VisitMenorigualRelopAST([NotNull] Parser1.MenorigualRelopASTContext context)
    {
        TreeNode menorigual = new TreeNode(context.MENORIGUAL().ToString());
        TreeNode[] arreglo = new TreeNode[] { menorigual };
        TreeNode final = new TreeNode("Relop-MenorIgual", arreglo);
        return final;
    }


    public override object VisitComparacionRelopAST([NotNull] Parser1.ComparacionRelopASTContext context)
    {
        TreeNode comparacion = new TreeNode(context.COMPARACION().ToString());
        TreeNode[] arreglo = new TreeNode[] { comparacion };
        TreeNode final = new TreeNode("Relop-Comparacion", arreglo);
        return final;
    }


    public override object VisitFormParsAST([NotNull] Parser1.FormParsASTContext context)
    {
        TreeNode type1 = (TreeNode)Visit(context.type(0));
        TreeNode ID1 = new TreeNode(context.ID(0).ToString());
        int largo = context.COMA().Count() + context.type().Count() + context.ID().Count();
        TreeNode[] arreglo = new TreeNode[largo];
        arreglo[0] = type1;
        arreglo[1] = ID1;
        int count = 2;
        int icoma = 0; //contador de coma
        for (int i = 1; i <= context.type().Count() - 1; i++)
        {
            TreeNode coma = new TreeNode(context.COMA(icoma).ToString());
            TreeNode type2 = (TreeNode)Visit(context.type(i));
            TreeNode ID2 = new TreeNode(context.ID(i).ToString());
            arreglo[count] = coma;
            count++;
            arreglo[count] = type2;
            count++;
            arreglo[count] = ID2;
            count++;
            icoma++;
        }
        TreeNode final = new TreeNode("FormPars", arreglo);
        return final;
    }


    public override object VisitActParsAST([NotNull] Parser1.ActParsASTContext context)
    {
        TreeNode expr1 = (TreeNode)Visit(context.expr(0));
        int largo = context.COMA().Count() + context.expr().Count();
        TreeNode[] arreglo = new TreeNode[largo];
        arreglo[0] = expr1;
        int count = 1; //contador del arreglo
        int icoma = 0; //contador para coma
        for (int i = 1; i <= context.expr().Count() - 1; i++)
        {
            TreeNode coma = new TreeNode(context.COMA(icoma).ToString());
            TreeNode expr2 = (TreeNode)Visit(context.expr(i));
            arreglo[count] = coma;
            arreglo[count + 1] = expr2;
            count += 2;
            icoma++;
        }
        TreeNode final = new TreeNode("Actpars", arreglo);
        return final;
    }


    public override object VisitDesignatorAST([NotNull] Parser1.DesignatorASTContext context)
    {
        TreeNode ID1 = new TreeNode(context.ID(0).ToString());
        TreeNode[] arreglo;
        if (context.PUNTO() != null)
        {
            int ipunto = 0; //contador para punto
            int largo = context.PUNTO().Count() + context.ID().Count();
            arreglo = new TreeNode[largo]; //inicializar arreglo
            arreglo[0] = ID1;
            int count = 1; //contador para arreglo
            for (int i = 1; i <= context.ID().Count() - 1; i++)
            {
                TreeNode punto = new TreeNode(context.PUNTO(ipunto).ToString());
                TreeNode ID2 = new TreeNode(context.ID(i).ToString());
                arreglo[count] = punto;
                arreglo[count + 1] = ID2;
                count += 2;
                ipunto++;

            }
        }
        else
        {
            int largo = 1 + context.PCUADRADO_IZQ().Count() + context.PCUADRADO_DER().Count() + context.expr().Count();
            arreglo = new TreeNode[largo];
            arreglo[0] = ID1;
            int count = 1; //contador de arreglo
            for (int i = 0; i <= context.expr().Count() - 1; i++)
            {
                TreeNode PI = new TreeNode(context.PCUADRADO_IZQ(i).ToString());
                TreeNode expr = (TreeNode)Visit(context.expr(i));
                TreeNode PD = new TreeNode(context.PCUADRADO_DER(i).ToString());
                arreglo[count] = PI;
                arreglo[count + 1] = expr;
                arreglo[count + 2] = PD;
                count += 3;
            }
        }
        TreeNode final = new TreeNode("Designator", arreglo);
        return final;
    }


    public override object VisitCondFactAST([NotNull] Parser1.CondFactASTContext context)
    {
        TreeNode expr1 = (TreeNode)Visit(context.expr(0));
        TreeNode relop = (TreeNode)Visit(context.relop());
        TreeNode expr2 = (TreeNode)Visit(context.expr(1));
        TreeNode[] arreglo = new TreeNode[] { expr1, relop, expr2 };
        TreeNode final = new TreeNode("Condfact", arreglo);
        return final;
    }


    public override object VisitConditionAST([NotNull] Parser1.ConditionASTContext context)
    {
        TreeNode condterm1 = (TreeNode)Visit(context.condTerm(0));
        int largo = context.O().Count() + context.condTerm().Count();
        TreeNode[] arreglo = new TreeNode[largo];
        arreglo[0] = condterm1;
        int count = 1; //contador del arreglo
        int iOr = 0; //contador para Or
        for (int i = 1; i <= context.condTerm().Count() - 1; i++)
        {
            TreeNode Or = new TreeNode(context.O(iOr).ToString());
            TreeNode condterm2 = (TreeNode)Visit(context.condTerm(i));
            arreglo[count] = Or;
            arreglo[count + 1] = condterm2;
            count += 2;
            iOr++;
        }
        TreeNode final = new TreeNode("Condition", arreglo);
        return final;
    }


    public override object VisitDivMulopAST([NotNull] Parser1.DivMulopASTContext context)
    {
        TreeNode div = new TreeNode(context.DIV().ToString());
        TreeNode[] arreglo = new TreeNode[] { div };
        TreeNode final = new TreeNode("Mulop-Div", arreglo);
        return final;
    }


    public override object VisitDivmodMulopAST([NotNull] Parser1.DivmodMulopASTContext context)
    {
        TreeNode divmod = new TreeNode(context.DIVMOD().ToString());
        TreeNode[] arreglo = new TreeNode[] { divmod };
        TreeNode final = new TreeNode("Mulop-DivMod", arreglo);
        return final;
    }


    public override object VisitMulMulopAST([NotNull] Parser1.MulMulopASTContext context)
    {
        TreeNode mul = new TreeNode(context.MUL().ToString());
        TreeNode[] arreglo = new TreeNode[] { mul };
        TreeNode final = new TreeNode("Mulop-Mul", arreglo);
        return final; ;
    }


    public override object VisitReadStatAST([NotNull] Parser1.ReadStatASTContext context)
    {
        TreeNode read = new TreeNode(context.READ().ToString());
        TreeNode PI = new TreeNode(context.PIZQ().ToString());
        TreeNode designator = (TreeNode)Visit(context.designator());
        TreeNode PD = new TreeNode(context.PDER().ToString());
        TreeNode pycoma = new TreeNode(context.PyCOMA().ToString());
        TreeNode[] arreglo = new TreeNode[] { read, PI, designator, PD, pycoma };
        TreeNode final = new TreeNode("Statement", arreglo);
        return final;
    }


    public override object VisitReturnStatAST([NotNull] Parser1.ReturnStatASTContext context)
    {
        TreeNode returns = new TreeNode(context.RETURN().ToString());
        TreeNode pycoma = new TreeNode(context.PyCOMA().ToString());
        TreeNode[] arreglo;
        if (context.expr() != null)
        {
            TreeNode expr = (TreeNode)Visit(context.expr());
            arreglo = new TreeNode[3];
            arreglo[0] = returns;
            arreglo[1] = expr;
            arreglo[2] = pycoma;
        }
        else
        {
            arreglo = new TreeNode[2];
            arreglo[0] = returns;
            arreglo[1] = pycoma;
        }
        TreeNode final = new TreeNode("Statement", arreglo);
        return final;
    }


    public override object VisitPyStatAST([NotNull] Parser1.PyStatASTContext context)
    {
        TreeNode pycoma = new TreeNode(context.PyCOMA().ToString());
        TreeNode[] arreglo = new TreeNode[] { pycoma };
        TreeNode final = new TreeNode("Statement", arreglo);
        return final;
    }


    public override object VisitWhileStatAST([NotNull] Parser1.WhileStatASTContext context)
    {
        TreeNode ciclo_while = new TreeNode(context.CICLO_WHILE().ToString());
        TreeNode PI = new TreeNode(context.PIZQ().ToString());
        TreeNode condition = (TreeNode)Visit(context.condition());
        TreeNode PD = new TreeNode(context.PDER().ToString());
        TreeNode statement = (TreeNode)Visit(context.statement());
        TreeNode[] arreglo = new TreeNode[] { ciclo_while, PI, condition, PD, statement };
        TreeNode final = new TreeNode("Statement", arreglo);
        return final;
    }


    public override object VisitWriteStatAST([NotNull] Parser1.WriteStatASTContext context)
    {
        TreeNode write = new TreeNode(context.WRITE().ToString());
        TreeNode PI = new TreeNode(context.PIZQ().ToString());
        TreeNode expr = (TreeNode)Visit(context.expr());
        TreeNode PD = new TreeNode(context.PDER().ToString());
        TreeNode pycoma = new TreeNode(context.PyCOMA().ToString());
        TreeNode[] arreglo;
        if (context.NUMBER() != null)
        {
            TreeNode number = new TreeNode(context.NUMBER().ToString());
            TreeNode coma = new TreeNode(context.COMA().ToString());
            arreglo = new TreeNode[] { write, PI, expr, coma, number, PD, pycoma };
        }
        else
        {
            arreglo = new TreeNode[] { write, PI, expr, PD, pycoma };
        }
        TreeNode final = new TreeNode("Statement", arreglo);
        return final;
    }


    public override object VisitForeachStatAST([NotNull] Parser1.ForeachStatASTContext context)
    {
        TreeNode ciclo_foreach = new TreeNode(context.CICLO_FOREACH().ToString());
        TreeNode PI = new TreeNode(context.PIZQ().ToString());
        TreeNode type = (TreeNode)Visit(context.type());
        TreeNode ID = new TreeNode(context.ID().ToString());
        TreeNode ins = new TreeNode(context.IN().ToString());
        TreeNode expr = (TreeNode)Visit(context.expr());
        TreeNode PD = new TreeNode(context.PDER().ToString());
        TreeNode statement = (TreeNode)Visit(context.statement());
        TreeNode[] arreglo = new TreeNode[] { ciclo_foreach, PI, type, ID, ins, expr, PD, statement };
        TreeNode final = new TreeNode("Statement", arreglo);
        return final;
    }


    public override object VisitDesignatorStatAST([NotNull] Parser1.DesignatorStatASTContext context)
    {
        TreeNode designator = (TreeNode)Visit(context.designator());
        TreeNode pycoma = new TreeNode(context.PyCOMA().ToString());
        TreeNode[] arreglo;
        int largo = 0;
        if (context.ASIGN() != null)
        {
            TreeNode asign = new TreeNode(context.ASIGN().ToString());
            TreeNode expr = (TreeNode)Visit(context.expr());
            largo += 4; //2 estáticos y 2 posibles
            arreglo = new TreeNode[largo];
            arreglo[0] = designator;
            arreglo[1] = asign;
            arreglo[2] = expr;
            arreglo[3] = pycoma;
        }
        else if (context.PIZQ() != null)
        {
            TreeNode PI = new TreeNode(context.PIZQ().ToString());
            TreeNode PD = new TreeNode(context.PDER().ToString());
            if (context.actPars() != null) //viene o no el actpars
            {
                TreeNode actpars = (TreeNode)Visit(context.actPars());
                largo += 5; //2 estáticos y 3 posibles
                arreglo = new TreeNode[largo];
                arreglo[0] = designator;
                arreglo[1] = PI;
                arreglo[2] = actpars;
                arreglo[3] = PD;
                arreglo[4] = pycoma;
            }
            else
            {
                largo += 4; //2 estaticos y 2 posibles
                arreglo = new TreeNode[largo];
                arreglo[0] = designator;
                arreglo[1] = PI;
                arreglo[2] = PD;
                arreglo[3] = pycoma;
            }
        }
        else if (context.INCRE() != null)
        {
            largo += 3; //2 estáticos 1 posible
            TreeNode incre = new TreeNode(context.INCRE().ToString());
            arreglo = new TreeNode[largo];
            arreglo[0] = designator;
            arreglo[1] = incre;
            arreglo[2] = pycoma;
        }
        else //preguntar si debo evaluar el DECRE del statement, porque si uso else if me da error el arreglo al mandarlo como nodo final
        {
            largo += 3; //2 estáticos 1 posible
            TreeNode decre = new TreeNode(context.DECRE().ToString());
            arreglo = new TreeNode[largo];
            arreglo[0] = designator;
            arreglo[1] = decre;
            arreglo[2] = pycoma;
        }
        TreeNode final = new TreeNode("Statement-Desig", arreglo);
        return final;
    }


    public override object VisitIfStatAST([NotNull] Parser1.IfStatASTContext context)
    {
        TreeNode cond_if = new TreeNode(context.CONDICION_IF().ToString());
        TreeNode PI = new TreeNode(context.PIZQ().ToString());
        TreeNode condition = (TreeNode)Visit(context.condition());
        TreeNode PD = new TreeNode(context.PDER().ToString());
        TreeNode statement1 = (TreeNode)Visit(context.statement(0));
        TreeNode[] arreglo;
        if (context.CONDICION_ELSE() != null) //puede o no venir la condición else
        {
            TreeNode cond_else = new TreeNode(context.CONDICION_ELSE().ToString());
            TreeNode statement2 = (TreeNode)Visit(context.statement(1)); //como viene o no viene y solo 1 vez se usa el 1 y no un ciclo
            arreglo = new TreeNode[] { cond_if, PI, condition, PD, statement1, cond_else, statement2 };
        }
        arreglo = new TreeNode[] { cond_if, PI, condition, PD, statement1 };
        TreeNode final = new TreeNode("statement", arreglo);
        return final;
    }


    public override object VisitForStatAST([NotNull] Parser1.ForStatASTContext context)
    {
        TreeNode ciclo_for = new TreeNode(context.CICLO_FOR().ToString());
        TreeNode PI = new TreeNode(context.PIZQ().ToString());
        TreeNode expr = (TreeNode)Visit(context.expr());
        TreeNode pycoma = new TreeNode(context.PyCOMA().ToString());
        TreeNode PD = new TreeNode(context.PDER().ToString());
        TreeNode statement2 = (TreeNode)Visit(context.statement(1));
        TreeNode[] arreglo;
        if (context.condition() != null) //existe condición
        {
            TreeNode condition = (TreeNode)Visit(context.condition());
            if (context.statement() != null) //existe un statement
            {
                arreglo = new TreeNode[9];
                TreeNode statement1 = (TreeNode)Visit(context.statement(0));
                arreglo[0] = ciclo_for;
                arreglo[1] = PI;
                arreglo[2] = expr;
                arreglo[3] = pycoma;
                arreglo[4] = condition;
                arreglo[5] = pycoma;
                arreglo[6] = statement1;
                arreglo[7] = PD;
                arreglo[8] = statement2;
            }
            else
            {
                arreglo = new TreeNode[8];
                TreeNode statement1 = (TreeNode)Visit(context.statement(0));
                arreglo[0] = ciclo_for;
                arreglo[1] = PI;
                arreglo[2] = expr;
                arreglo[3] = pycoma;
                arreglo[4] = condition;
                arreglo[5] = pycoma;
                arreglo[6] = PD;
                arreglo[7] = statement2;
            }

        }
        else
        {
            if (context.statement() != null) //existe un statement
            {
                arreglo = new TreeNode[8];
                TreeNode statement1 = (TreeNode)Visit(context.statement(0));
                arreglo[0] = ciclo_for;
                arreglo[1] = PI;
                arreglo[2] = expr;
                arreglo[3] = pycoma;
                arreglo[4] = pycoma;
                arreglo[5] = statement1;
                arreglo[6] = PD;
                arreglo[7] = statement2;
            }
            else
            {
                arreglo = new TreeNode[7];
                TreeNode statement1 = (TreeNode)Visit(context.statement(0));
                arreglo[0] = ciclo_for;
                arreglo[1] = PI;
                arreglo[2] = expr;
                arreglo[3] = pycoma;
                arreglo[4] = pycoma;
                arreglo[5] = PD;
                arreglo[6] = statement2;
            }

        }
        TreeNode final = new TreeNode("Statement", arreglo);
        return final;
    }


    public override object VisitBlockStatAST([NotNull] Parser1.BlockStatASTContext context)
    {
        TreeNode block = (TreeNode)Visit(context.block());
        TreeNode[] arreglo = new TreeNode[] { block };
        TreeNode final = new TreeNode("Statement", arreglo);
        return final;
    }


    public override object VisitBreakStatAST([NotNull] Parser1.BreakStatASTContext context)
    {
        TreeNode breaks = new TreeNode(context.BREAK().ToString());
        TreeNode pycoma = new TreeNode(context.PyCOMA().ToString());
        TreeNode[] arreglo = new TreeNode[] { breaks, pycoma };
        TreeNode final = new TreeNode("Statement", arreglo);
        return final;
    }


    public override object VisitBlockAST([NotNull] Parser1.BlockASTContext context)
    {
        TreeNode CD = new TreeNode(context.COR_DER().ToString());
        TreeNode CI = new TreeNode(context.COR_IZQ().ToString());
        int largo = 2 + context.statement().Count();
        TreeNode[] arreglo = new TreeNode[largo];
        arreglo[0] = CD;
        int count = 1;
        for (int i = 0; i <= context.statement().Count() - 1; i++)
        {
            TreeNode statement = (TreeNode)Visit(context.statement(i));
            arreglo[count] = statement;
            count++;
        }
        arreglo[count] = CI;
        TreeNode final = new TreeNode("Block", arreglo);
        return final;
    }


    public override object VisitExprAST([NotNull] Parser1.ExprASTContext context)
    {
        TreeNode[] arreglo;
        if (context.RESTA() != null)
        {
            TreeNode resta = new TreeNode(context.RESTA().ToString());
            TreeNode term1 = (TreeNode)Visit(context.term(0));
            int largo = 1 + context.term().Count() + context.addop().Count();
            arreglo = new TreeNode[largo];
            arreglo[0] = resta;
            arreglo[1] = term1;
            int count = 2; //contador de arreglo
            int iaddop = 0; //contador de mulop
            for (int i = 1; i <= context.term().Count() - 1; i++)
            {
                TreeNode addop = (TreeNode)Visit(context.addop(iaddop));
                TreeNode term2 = (TreeNode)Visit(context.term(i));
                arreglo[count] = addop;
                arreglo[count + 1] = term2;
                count += 2;
                iaddop++;
            }
        }
        else//no viene resta
        {
            TreeNode term1 = (TreeNode)Visit(context.term(0));
            int largo = context.term().Count() + context.addop().Count();
            arreglo = new TreeNode[largo];
            arreglo[0] = term1;
            int count = 1; //contador de arreglo
            int iaddop = 0; //contador de mulop
            for (int i = 1; i <= context.term().Count() - 1; i++)
            {
                TreeNode addop = (TreeNode)Visit(context.addop(iaddop));
                TreeNode term2 = (TreeNode)Visit(context.term(i));
                arreglo[count] = addop;
                arreglo[count + 1] = term2;
                count += 2;
                iaddop++;
            }
        }
        TreeNode final = new TreeNode("Expr", arreglo);
        return final;
    }


    public override object VisitTermAST([NotNull] Parser1.TermASTContext context)
    {
        TreeNode factor1 = (TreeNode)Visit(context.factor(0));
        int largo = context.mulop().Count() + context.factor().Count();
        TreeNode[] arreglo = new TreeNode[largo];
        arreglo[0] = factor1;
        int count = 1; //contador del arreglo
        int iMulop = 0; //contador para mulop
        for (int i = 1; i <= context.factor().Count() - 1; i++)
        {
            TreeNode mulop = (TreeNode)Visit(context.mulop(iMulop));
            TreeNode factor2 = (TreeNode)Visit(context.factor(i));
            arreglo[count] = mulop;
            count++;
            arreglo[count] = factor2;
            count++; ;
            iMulop++;
        }
        TreeNode final = new TreeNode("Factor", arreglo);
        return final;
    }


    public override object VisitExprFactorAST([NotNull] Parser1.ExprFactorASTContext context)
    {
        TreeNode PI = new TreeNode(context.PIZQ().ToString());
        TreeNode expr = (TreeNode)Visit(context.expr());
        TreeNode PD = new TreeNode(context.PDER().ToString());
        TreeNode[] arreglo = new TreeNode[] { PI, expr, PD };
        TreeNode final = new TreeNode("Factor-Expr", arreglo);
        return final;
    }


    public override object VisitTruefalseFactorAST([NotNull] Parser1.TruefalseFactorASTContext context)
    {
        TreeNode[] arreglo;
        if (context.TRUE() != null)
        {
            TreeNode trues = new TreeNode(context.TRUE().ToString());
            arreglo = new TreeNode[] { trues };
        }
        else
        {
            TreeNode falses = new TreeNode(context.FALSE().ToString());
            arreglo = new TreeNode[] { falses };
        }
        TreeNode final = new TreeNode("Factor-TrueFalse", arreglo);
        return final;
    }


    public override object VisitNewFactorAST([NotNull] Parser1.NewFactorASTContext context)
    {
        TreeNode news = new TreeNode(context.NEW().ToString());
        TreeNode ID = new TreeNode(context.ID().ToString());
        TreeNode[] arreglo;
        if (context.PCUADRADO_IZQ() != null)
        {
            TreeNode PI = new TreeNode(context.PCUADRADO_IZQ().ToString());
            TreeNode expr = (TreeNode)Visit(context.expr());
            TreeNode PD = new TreeNode(context.PCUADRADO_DER().ToString());
            arreglo = new TreeNode[] { news, ID, PI, expr, PD };
        }
        arreglo = new TreeNode[] { news, ID };
        TreeNode final = new TreeNode("Factor-New", arreglo);
        return final;
    }


    public override object VisitDesignatorFactorAST([NotNull] Parser1.DesignatorFactorASTContext context)
    {
        TreeNode designator = (TreeNode)Visit(context.designator());
        TreeNode[] arreglo;
        if (context.PIZQ() != null)
        {
            if (context.actPars() != null)
            {
                TreeNode PI = new TreeNode(context.PIZQ().ToString());
                TreeNode actpars = (TreeNode)Visit(context.actPars());
                TreeNode PD = new TreeNode(context.PDER().ToString());
                arreglo = new TreeNode[4];
                arreglo[0] = designator;
                arreglo[1] = PI;
                arreglo[2] = actpars;
                arreglo[3] = PD;

            }
            else
            {
                TreeNode PI = new TreeNode(context.PIZQ().ToString());
                TreeNode PD = new TreeNode(context.PDER().ToString());
                arreglo = new TreeNode[3];
                arreglo[0] = designator;
                arreglo[1] = PI;
                arreglo[2] = PD;
            }
        }
        else
        {
            arreglo = new TreeNode[1];
            arreglo[0] = designator;
        }
        TreeNode final = new TreeNode("Factor-Designator", arreglo);
        return final;
    }


    public override object VisitFloatFactorAST([NotNull] Parser1.FloatFactorASTContext context)
    {
        TreeNode flotante = new TreeNode(context.FLOAT().ToString());
        TreeNode[] arreglo = new TreeNode[] { flotante };
        TreeNode final = new TreeNode("Factor-Float", arreglo);
        return final;
    }

    public override object VisitNumberFactorAST([NotNull] Parser1.NumberFactorASTContext context)
    {
        TreeNode number = new TreeNode(context.NUMBER().ToString());
        TreeNode[] arreglo = new TreeNode[] { number };
        TreeNode final = new TreeNode("Factor-Number", arreglo);
        return final;
    }



    public override object VisitCharconstFactorAST([NotNull] Parser1.CharconstFactorASTContext context)
    {
        TreeNode charconst = new TreeNode(context.CharConst().ToString());
        TreeNode[] arreglo = new TreeNode[] { charconst };
        TreeNode final = new TreeNode("Factor-Charconst", arreglo);
        return final;
    }


    public override object VisitVarDeclAST([NotNull] Parser1.VarDeclASTContext context)
    {
        TreeNode type = (TreeNode)Visit(context.type());
        TreeNode ID = new TreeNode(context.ID(0).ToString());
        TreeNode pycoma = new TreeNode(context.PyCOMA().ToString());
        int largo = 2 + context.ID().Count() + context.COMA().Count();
        TreeNode[] arreglo = new TreeNode[largo];
        arreglo[0] = type;
        arreglo[1] = ID;
        int cont = 2; //arreglo
        int i2 = 0; //coma
        for (int i = 1; i <= context.ID().Count() - 1; i++)
        {
            TreeNode coma = new TreeNode(context.COMA(i2).ToString());
            TreeNode ID2 = new TreeNode(context.ID(i).ToString());
            arreglo[cont] = coma;
            arreglo[cont + 1] = ID2;
            cont += 2;
            i2++;
        }
        arreglo[cont] = pycoma;
        TreeNode final = new TreeNode("varDecl", arreglo);
        return final;
    }
}
