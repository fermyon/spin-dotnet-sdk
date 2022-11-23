use heck::ToPascalCase;

wit_bindgen_rust::export!("./custom-filter.wit");

struct CustomFilter;

impl custom_filter::CustomFilter for CustomFilter {
    fn exec(source: String) -> Result<String, String> {
        let result = source.split('.').map(|s| s.to_pascal_case()).collect::<Vec<_>>().join(".");
        Ok(result)
    }
}

#[cfg(test)]
mod test {
    use crate::custom_filter::CustomFilter;

    // Just to save cluttering the tests with ceremonial bits
    fn exec_filter(source: &str) -> Result<String, String> {
        super::CustomFilter::exec(source.to_owned())
    }

    #[test]
    fn test_dotted_pascal_case() {
        assert_eq!("Fermyon.PetStore", exec_filter("Fermyon.PetStore").unwrap());
        assert_eq!("FermyonPetStore", exec_filter("fermyon-pet-store").unwrap());
        assert_eq!("FermyonPetStore", exec_filter("fermyon_pet_store").unwrap());
        assert_eq!("Fermyon.PetStore", exec_filter("fermyon.pet-store").unwrap());
        assert_eq!("Fermyon.PetStore", exec_filter("fermyon.pet_store").unwrap());
        assert_eq!("FermyonPetStore", exec_filter("fermyon pet store").unwrap());
    }
}
