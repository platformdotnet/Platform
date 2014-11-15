namespace Platform
{
	public delegate R Func<in T1, in T2, in T3, in T4, in T5, out R>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5);
	public delegate R Func<in T1, in T2, in T3, in T4, in T5, in T6, out R>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6);
	public delegate R Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, out R>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7);
	public delegate R Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, out R>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 t8);
}
