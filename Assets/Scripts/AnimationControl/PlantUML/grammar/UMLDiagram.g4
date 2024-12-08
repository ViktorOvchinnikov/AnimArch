grammar UMLDiagram;


diagram
    :   '@startuml' element* '@enduml' EOF 
    ;

element
    :   classDefinition
    |   relation
    |   labeledRelation
    ;

classDefinition
    :   'class' className '{' classBody '}'
    ;

classBody
    :   classMember*
    ;

classMember
    :   method
    |   attribute
    ;

method
    :   visibility? methodName '(' methodArguments? ')' ':' returnType
    ;

attribute
    :   visibility? attributeName (':' type)?
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
    :   className relationType className
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

TEXT
    :   ~["\r\n]+
    ;

className
    :   NAME
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
