
SimpleExtps.dll: dlldata.obj SimpleExt_p.obj SimpleExt_i.obj
	link /dll /out:SimpleExtps.dll /def:SimpleExtps.def /entry:DllMain dlldata.obj SimpleExt_p.obj SimpleExt_i.obj \
		kernel32.lib rpcndr.lib rpcns4.lib rpcrt4.lib oleaut32.lib uuid.lib \

.c.obj:
	cl /c /Ox /DWIN32 /D_WIN32_WINNT=0x0400 /DREGISTER_PROXY_DLL \
		$<

clean:
	@del SimpleExtps.dll
	@del SimpleExtps.lib
	@del SimpleExtps.exp
	@del dlldata.obj
	@del SimpleExt_p.obj
	@del SimpleExt_i.obj
