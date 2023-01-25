.PHONY: generate
generate:
	# wit-bindgen c --export wit/ephemeral/spin-http.wit --out-dir ./src/native/
	# wit-bindgen c --import wit/ephemeral/wasi-outbound-http.wit --out-dir ./src/native/
	# wit-bindgen c --import wit/ephemeral/outbound-redis.wit --out-dir ./src/native
	# wit-bindgen c --import wit/ephemeral/outbound-pg.wit --out-dir ./src/native
	# wit-bindgen c --import wit/ephemeral/spin-config.wit --out-dir ./src/native

.PHONY: bootstrap
bootstrap:
	# install the WIT Bindgen version we are currently using in Spin v0.7.1
	cargo install wit-bindgen-cli --git https://github.com/bytecodealliance/wit-bindgen --rev cb871cfa1ee460b51eb1d144b175b9aab9c50aba --force
	cargo install wizer --git https://github.com/bytecodealliance/wizer --rev 04e49c989542f2bf3a112d60fbf88a62cce2d0d0 --all-features --force
