import React, { useContext } from 'react';
import OriginalTabs from '@theme-original/Tabs';
import { GlobalTabContext } from '../GlobalTabContext';
import { CODE_TAB_GROUP_ID, DEFAULT_LANGUAGE } from '../codeTabs';

export default function TabsWrapper(props) {
	const { value } = useContext(GlobalTabContext);
	const isLanguageTabs = props.groupId === CODE_TAB_GROUP_ID;

	const forwardedProps = {
		...props,
	};

	if (isLanguageTabs) {
		forwardedProps.defaultValue = forwardedProps.defaultValue ?? value ?? DEFAULT_LANGUAGE;
	}

	return <OriginalTabs {...forwardedProps} />;
}
