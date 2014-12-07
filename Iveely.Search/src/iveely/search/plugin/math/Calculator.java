package iveely.search.plugin.math;

import java.util.Collections;
import java.util.Stack;

/**
 * Calculator.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-16 10:28:06
 */
public class Calculator {

    /**
     * Postfix stack.
     */
    private final Stack<String> postfixStack;

    /**
     * Operator Stack.
     */
    private final Stack<Character> opStack;

    /**
     * Operators use the ASCII -40 indexing of operator precedence.
     */
    private final int[] operatPriority;

    public Calculator() {
        postfixStack = new Stack<>();
        opStack = new Stack<>();
        operatPriority = new int[]{0, 3, 2, 1, -1, 1, 0, 2};
    }

    /**
     * According to the given expression evaluates.
     *
     * @param expression
     * @return
     */
    public String calculate(String expression) {
        try {
            Stack<String> resultStack = new Stack<>();
            prepare(expression);
            Collections.reverse(postfixStack);
            String firstValue, secondValue, currentValue;
            while (!postfixStack.isEmpty()) {
                currentValue = postfixStack.pop();
                if (!isOperator(currentValue.charAt(0))) {
                    resultStack.push(currentValue);
                } else {
                    secondValue = resultStack.pop();
                    firstValue = resultStack.pop();
                    String tempResult = calculate(firstValue, secondValue, currentValue.charAt(0));
                    resultStack.push(tempResult);
                }
            }
            return expression + "=" + Double.valueOf(resultStack.pop());
        } catch (NumberFormatException e) {
        }
        return "";
    }

    /**
     * Be converted into postfix expression stack.
     *
     * @param expression
     */
    private void prepare(String expression) {
        opStack.push(',');
        char[] arr = expression.toCharArray();
        int currentIndex = 0;
        int count = 0;
        char currentOp, peekOp;
        for (int i = 0; i < arr.length; i++) {
            currentOp = arr[i];
            if (isOperator(currentOp)) {
                if (count > 0) {
                    postfixStack.push(new String(arr, currentIndex, count));
                }
                peekOp = opStack.peek();
                if (currentOp == ')') {
                    while (opStack.peek() != '(') {
                        postfixStack.push(String.valueOf(opStack.pop()));
                    }
                    opStack.pop();
                } else {
                    while (currentOp != '(' && peekOp != ',' && compare(currentOp, peekOp)) {
                        postfixStack.push(String.valueOf(opStack.pop()));
                        peekOp = opStack.peek();
                    }
                    opStack.push(currentOp);
                }
                count = 0;
                currentIndex = i + 1;
            } else {
                count++;
            }
        }
        if (count > 1 || (count == 1 && !isOperator(arr[currentIndex]))) {
            postfixStack.push(new String(arr, currentIndex, count));
        }
        while (opStack.peek() != ',') {
            postfixStack.push(String.valueOf(opStack.pop()));
        }
    }

    /**
     * Determine whether the arithmetic sign.
     *
     * @param c
     * @return
     */
    private boolean isOperator(char c) {
        return c == '+' || c == '-' || c == '*' || c == '/' || c == '(' || c == ')';
    }

    /**
     * Use ASCII code -40 subscript to do arithmetic signs priority.
     *
     * @param cur
     * @param peek
     * @return
     */
    public boolean compare(char cur, char peek) {
        boolean result = false;
        if (operatPriority[(peek) - 40] >= operatPriority[(cur) - 40]) {
            result = true;
        }
        return result;
    }

    /**
     * According to the given arithmetic operators to do the calculation.
     *
     * @param firstValue
     * @param secondValue
     * @param currentOp
     * @return
     */
    private String calculate(String firstValue, String secondValue, char currentOp) {
        String result = "";
        switch (currentOp) {
            case '+':
                result = String.valueOf(ArithHelper.add(firstValue, secondValue));
                break;
            case '-':
                result = String.valueOf(ArithHelper.sub(firstValue, secondValue));
                break;
            case '*':
                result = String.valueOf(ArithHelper.mul(firstValue, secondValue));
                break;
            case '/':
                result = String.valueOf(ArithHelper.div(firstValue, secondValue));
                break;
            default:
                result = "error format.";
        }
        return result;
    }
}
