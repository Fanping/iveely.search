package com.iveely.framework.text;

/**
 * B+ tree.
 *
 * @author liufanping@iveely.com
 * @date 2014-12-27 18:16:54
 */
public class BPlusTree {

    /**
     * 根节点
     */
    protected BPlusTreeNode root;

    /**
     * 阶数，M值
     */
    protected int order;

    /**
     * 叶子节点的链表头
     */
    protected BPlusTreeNode head;

    public BPlusTreeNode getHead() {
        return head;
    }

    public void setHead(BPlusTreeNode head) {
        this.head = head;
    }

    public BPlusTreeNode getRoot() {
        return root;
    }

    public void setRoot(BPlusTreeNode root) {
        this.root = root;
    }

    public int getOrder() {
        return order;
    }

    public void setOrder(int order) {
        this.order = order;
    }

    public Object get(Comparable key) {
        return root.get(key);
    }

    public void remove(Comparable key) {
        root.remove(key, this);

    }

    public void insertOrUpdate(Comparable key, Object obj) {
        root.insertOrUpdate(key, obj, this);

    }

    public BPlusTree(int order) {
        if (order < 3) {
            System.out.print("order must be greater than 2");
            System.exit(0);
        }
        this.order = order;
        root = new BPlusTreeNode(true, true);
        head = root;
    }
}
