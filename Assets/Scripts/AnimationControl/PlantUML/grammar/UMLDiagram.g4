grammar UMLDiagram;


diagram
    :   '@startuml' element* '@enduml' EOF 
    ;

element
    :   package
    |   classDefinition
    |   interfaceDefinition
    |   abstractClassDefinition
    |   enumDefinition
    |   note
    |   relation
    |   labeledRelation
    ;

classDefinition
    :   'class' className ('as' aliasName)? '{' classBody '}'
    ;

classBody
    :   classMember*
    ;

classMember
    :   method
    |   attribute
    ;

attribute
    :   visibility? ('{static}' | '{abstract}')? attributeName (':' type)?
    ;

method
    :   visibility? ('{static}' | '{abstract}')? methodName '(' methodArguments? ')' ':' returnType
    ;

methodArguments
    :   methodArgument nextArg*
    ;

nextArg
    :   ',' methodArgument 
    ;

methodArgument
    :   type argumentName
    ;

relation
    :   className '[' multiplicity? ']' relationType '[' multiplicity? ']' className
    ;


labeledRelation
    :   className label relationType label className description
    ;

label
    :   '"' TEXT '"'
    ;

description
    :   ':' TEXT
    ;

relationType
    :   '<|..'
    |   '-->'
    |   '<--'
    |   '<-->'
    |   '--'
    |   '..>'
    |   '<|--'
    |   '<|..'
    ;

visibility
    :   '+'
    |   '-'
    |   '#'
    ;

multiplicity
    :   '*' 
    |   '1' 
    |   '0..1' 
    |   '1..*'
    |   'n'
    ;

TEXT
    :   ~["\r\n]+
    ;

className
    :   NAME ('<' type (',' type)* '>')?
    ;

methodName
    :   NAME
    ;

attributeName
    :   NAME
    ;

argumentName
    :   NAME
    ;

returnType
    :   NAME
    ;

type
    :   NAME
    ;

NAME
    :   [a-zA-Z][a-zA-Z0-9_]*
    ;

COMMENT
    :   '//' ~('\r' | '\n')*
    ;

WHITE_SPACE
    :   [ \t\r\n]+
    -> skip
    ;
	
interfaceDefinition
    :   'interface' className '{' classBody '}'
    ;

abstractClassDefinition
    :   'abstract' 'class' className '{' classBody '}'
    ;

enumDefinition
    :   'enum' className '{' enumBody '}'
    ;

enumBody
    :   enumValue (',' enumValue)* 
    ;

enumValue
    :   NAME
    ;
	
note
    :   'note' label 'as' NAME
    ;

package
    :   'package' packageName '{' element* '}'
    ;

packageName
    :   NAME
    ;
	
aliasName
    :   NAME
    ;