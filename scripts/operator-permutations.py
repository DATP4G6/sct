from itertools import permutations
perms = permutations(['*', '/', '%', '+', '-', '>', '<', '>=', '<=', '==', '!=', '&&', '||'], 2)
#for p in perms:
#    print(p)

cases = [f'    a = 1 {p[0]} 2 {p[1]} 3;\n' for p in perms]

with open('ExpressionBinary.sct', 'w') as file:
    file.write('function setup() -> void {\n')
    file.write('int ')
    for case in cases:
        file.write(case)
    file.write('}')
