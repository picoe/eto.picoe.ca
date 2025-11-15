import React from 'react';
import OriginalNavbarItem from '@theme-original/NavbarItem';
import LanguageSelectorNavbarItem from './LanguageSelectorNavbarItem';

export default function NavbarItemWrapper(props) {
	if (props.type === 'custom-languageSelector') {
		return <LanguageSelectorNavbarItem {...props} />;
	}
	return <OriginalNavbarItem {...props} />;
}
