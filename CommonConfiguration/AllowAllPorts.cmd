netsh advfirewall firewall add rule name="AllowAll" dir=in action=allow protocol=TCP localport=0-65535
subst q: %installdir:~0,-1%
