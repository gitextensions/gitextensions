#!/usr/bin/env python3
# parity-scaffolding: observe the real portal's file selection on an isolated session bus.
import json
from pathlib import Path
import sys

from gi.repository import Gio, GLib

log_path, ready_path = map(Path, sys.argv[1:])
interface = Gio.DBusNodeInfo.new_for_xml('''<node>
<interface name="org.freedesktop.FileManager1">
  <method name="ShowItems"><arg type="as" direction="in"/><arg type="s" direction="in"/></method>
  <method name="ShowFolders"><arg type="as" direction="in"/><arg type="s" direction="in"/></method>
</interface>
</node>''')


def called(connection, sender, object_path, interface_name, method, parameters, invocation):
    uris, startup_id = parameters.unpack()
    with log_path.open('a', encoding='utf-8') as output:
        output.write(json.dumps({'method': method, 'uris': uris}) + '\n')
    invocation.return_value(GLib.Variant('()', ()))


def acquired(connection, name):
    connection.register_object('/org/freedesktop/FileManager1', interface.interfaces[0], called, None, None)


def owned(connection, name):
    ready_path.write_text('ready\n', encoding='utf-8')


def lost(connection, name):
    raise RuntimeError('could not own isolated FileManager1 name')


Gio.bus_own_name(Gio.BusType.SESSION, 'org.freedesktop.FileManager1', Gio.BusNameOwnerFlags.NONE,
                 acquired, owned, lost)
GLib.MainLoop().run()
