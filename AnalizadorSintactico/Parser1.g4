
parser grammar Parser1;

@header {
using System;
}

options {
language = CSharp;
tokenVocab = Lexer1;
}
/*
* Parser Rules
*/


program 
: CLASE ID (constDecl | varDecl | classDecl)* COR_DER (methodDecl)* COR_IZQ						#programAST
;



constDecl
: CONSTANTE type ID ASIGN (NUMBER | CharConst|FLOAT) PyCOMA								#constDeclAST
;


varDecl
: type ID (COMA ID)* PyCOMA																		#varDeclAST
;



classDecl
: CLASE ID COR_DER (varDecl)* COR_IZQ															#classDeclAST
;



methodDecl
: (type | VOID) ID PIZQ (formPars)? PDER (varDecl)* block										#methodDeclAST
;


formPars
: type ID (COMA type ID)*																		#formParsAST
;

type
: ID (PCUADRADO_IZQ PCUADRADO_DER)?																#typeAST
;

statement
: designator (ASIGN expr | PIZQ (actPars)? PDER | INCRE | DECRE) PyCOMA							#designatorStatAST
| CONDICION_IF PIZQ condition PDER statement (CONDICION_ELSE statement)?						#ifStatAST
| CICLO_FOR PIZQ expr PyCOMA (condition)? PyCOMA (statement)? PDER statement					#forStatAST
| CICLO_WHILE PIZQ condition PDER statement														#whileStatAST
| CICLO_FOREACH PIZQ type ID IN expr PDER statement												#foreachStatAST
| BREAK PyCOMA																					#breakStatAST
| RETURN (expr)? PyCOMA																			#returnStatAST
| READ PIZQ designator PDER PyCOMA																#readStatAST
| WRITE PIZQ expr (COMA NUMBER)? PDER PyCOMA													#writeStatAST
| block																							#blockStatAST
| PyCOMA																						#pyStatAST

;


block
: COR_DER (statement)* COR_IZQ																	#blockAST
;

actPars
: expr (COMA expr)*																				#actParsAST
;

condition
: condTerm (O condTerm)*																		#conditionAST
;

condTerm
: condFact (Y condFact)*																		#condTermAST
;
/*preguntar si lleva |*/
condFact
: expr relop expr 																				#condFactAST
;

expr
: (RESTA)? term (addop term)*																	#exprAST
;

term
: factor (mulop factor)*																		#termAST
;

factor
: designator (PIZQ (actPars)? PDER)?															#designatorFactorAST
| NUMBER																						#numberFactorAST
| FLOAT																							#floatFactorAST
| CharConst																						#charconstFactorAST
| (TRUE | FALSE)																				#truefalseFactorAST
| NEW ID (PCUADRADO_IZQ expr PCUADRADO_DER)?													#newFactorAST
| PIZQ expr PDER																				#exprFactorAST
;

designator
: ID (PUNTO ID | PCUADRADO_IZQ expr PCUADRADO_DER)*												#designatorAST
;

relop
: COMPARACION																					#comparacionRelopAST
| DIFERENTE																						#diferenteRelopAST
| MAYOR																							#mayorRelopAST
| MAYORIGUAL																					#mayorigualRelopAST
| MENOR																							#menorRelopAST
| MENORIGUAL																					#menorigualRelopAST
;

addop
: SUMA																							#sumaAddopAST
| RESTA																							#restaAddopAST
;

mulop
: MUL																							#mulMulopAST
| DIV																							#divMulopAST
| DIVMOD																						#divmodMulopAST
;
