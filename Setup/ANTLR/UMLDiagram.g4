grammar UMLDiagram;


diagram
    :   '@startuml' element* '@enduml' EOF
    ;

element
    :   classDefinition
    |   relation
    ;

classDefinition
    :   'class' className '{' classBody '}'
    ;

classBody
    :   classMember*
    ;

classMember
    :   method
    ;

method
    :   visibility methodName '(' methodArguments? ')' ':' returnType
    ;

methodArguments
    :   methodArgument (',' methodArgument)*
    ;

methodArgument
    :   type argumentName
    ;

relation
    :   className relationType className
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

className
    :   NAME
    ;

methodName
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
