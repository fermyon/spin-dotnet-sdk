.PHONY: generate
generate:
	# wit-bindgen c --export wit/ephemeral/spin-http.wit --out-dir ./src/native/
	# wit-bindgen c --import wit/ephemeral/wasi-outbound-http.wit --out-dir ./src/native/
	wit-bindgen c --import wit/ephemeral/test.wit --out-dir ./src/native/

.PHONY: bootstrap
bootstrap:
	# install the WIT Bindgen version we are currently using in Spin e06c6b1
	cargo install wit-bindgen-cli --git https://github.com/bytecodealliance/wit-bindgen --rev dde4694aaa6acf9370206527a798ac4ba6a8c5b8 --force
