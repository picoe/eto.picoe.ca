import React, { useCallback, useEffect, useMemo, useState } from 'react';
import Head from '@docusaurus/Head';
import { useStorageSlot } from '@docusaurus/theme-common';
import { GlobalTabContext } from './GlobalTabContext';
import { DEFAULT_LANGUAGE, TAB_STORAGE_KEY } from './codeTabs';

export default function Root({ children }) {
	const [storedValue, storageSlot] = useStorageSlot(TAB_STORAGE_KEY);
	const [value, setValueState] = useState(storedValue ?? DEFAULT_LANGUAGE);

	useEffect(() => {
		if (storedValue && storedValue !== value) {
			setValueState(storedValue);
		}
	}, [storedValue, value]);

	const setValue = useCallback(
		(next) => {
			setValueState(next);
			storageSlot.set(next);
		},
		[storageSlot]
	);

	const contextValue = useMemo(() => ({ value, setValue }), [value, setValue]);

	return (
		<GlobalTabContext.Provider value={contextValue}>
			<Head>
				<link rel="icon" type="image/png" href="/img/icon-512.png" sizes="512x512" />
				<link rel="apple-touch-icon" sizes="180x180" href="/img/apple-touch-icon.png" />
			</Head>
			{children}
		</GlobalTabContext.Provider>
	);
}
