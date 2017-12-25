class A {
    public <T> void foo() {
    }
}

class B extends A {
    @Override
    public <T> void foo() {
    }
}

class C extends A {
    public <T> @Override void foo() {
    }
}